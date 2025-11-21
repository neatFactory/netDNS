namespace Aicrosoft.DDNSJob;


[KeyedName]
public sealed class RouterEventer(IServiceProvider serviceProvider) : Eventer<TimeJobContext>(serviceProvider)
{
    public override async Task StartAsync(TimeJobContext? jobContext, CancellationToken cancellationToken)
    {
        await base.StartAsync(jobContext, cancellationToken);

        var receiver = new UdpReceiver(ServiceProvider, cancellationToken);
        receiver.OnMessage += async (s, e) =>
        {
            //Magic={e.Datagram.Magic},
            Logger.LogDebug($"IPMeesageEventArgs --> {e.ToJson()}");
            var workerName = jobContext.GetWorkerName();
            var worker = ServiceProvider.GetKeyedService<IWorker<TimeJobContext>>(workerName);
            if (worker == null)
            {
                Logger.LogError($"{this} No Worker[{workerName}<{nameof(TimeJobContext)}>] was found. Please check the configuration or whether a Worker with this name has been implemented.");
                return;
            }

            var dnsrst = jobContext!.GetData<DDNSState>() ?? new DDNSState();
            Logger.LogDebug($"Wake up the Worker:{workerName} .");
            if (e.IpStyle == IpStyle.IPV4)
                dnsrst.QueryIpV4 = e.IPAddress;
            //jobContext.QueryIpV4 = e.IPAddress;
            if (e.IpStyle == IpStyle.IPV6)
                dnsrst.QueryIpV6 = e.IPAddress;
            //jobContext.QueryIpV6 = e.IPAddress;

            jobContext.SetData(dnsrst);
            await worker.StartAsync(jobContext, cancellationToken);
        };
        Logger.LogDebug($"Register RouteEvent OnMessage event is ok.");
        await Task.CompletedTask;
    }
}

/// <summary>
/// 定义解析后的数据模型
/// </summary>
public readonly struct UdpDatagram(int magic, string body, IPEndPoint remote)
{
    public readonly int Magic = magic;   // 演示：前 4 字节
    public readonly string Body = body;    // 演示：后面当字符串
    public readonly IPEndPoint RemoteEndPoint = remote;
}

/// <summary>
/// 事件参数：只把字符串和远程端点带出去
/// </summary>
public sealed class IPMeesageEventArgs(string ipAddress, IpStyle ipStyle) : EventArgs
{

    public string IPAddress { get; } = ipAddress;

    public IpStyle IpStyle { get; } = ipStyle;

}


/// <summary>
/// UDP 监听器
/// </summary>
public sealed class UdpReceiver : ServiceBase, IAsyncDisposable
{
    private readonly UdpClient udp;
    private readonly Task receiveTask;
    //private readonly DDNSOption setting;
    private readonly Regex? ipv4Reg;
    //private readonly Regex? ipv6Reg;

    /// <summary>
    /// 有新消息时触发
    /// </summary>
    public event EventHandler<IPMeesageEventArgs>? OnMessage;

    public UdpReceiver(IServiceProvider serviceProvider, CancellationToken token = default) : base(serviceProvider)
    {
        var setting = serviceProvider.GetRequiredService<IOptions<DDNSOption>>().Value;
        //Logger.LogDebug($"UdpServer setting:{setting.ToJson()}");

        if (setting.RouteEventSetting.IpV4RegPartten == null)
            ipv4Reg = null;
        else
            ipv4Reg = new Regex(setting.RouteEventSetting.IpV4RegPartten, RegexOptions.Compiled);

        //if (setting.RouteEventSetting.IpV6RegPartten == null)
        //    ipv6Reg = null;
        //else
        //    ipv6Reg = new Regex(setting.RouteEventSetting.IpV6RegPartten, RegexOptions.Compiled);

        udp = new UdpClient(setting.RouteEventSetting.UpdServicePort);
        Logger.LogInformation($"DDNS RouterEventer start listening on port {setting.RouteEventSetting.UpdServicePort} of the local UDP service.");

        // 立即启动后台接收循环
        receiveTask = Task.Run(() => ReceiveLoop(token), token);
    }

    //public ChannelReader<UdpDatagram> Reader => udpChannel.Reader;

    private async Task ReceiveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 等待数据
                var rs = await udp.ReceiveAsync(token);
                // 解析
                var datagram = Parse(rs.Buffer, rs.RemoteEndPoint);
                Logger.LogTrace($"UdpClient[{datagram.RemoteEndPoint}] --> Message:{datagram.Body} .");

                //ipv4 capture
                if (ipv4Reg != null && ipv4Reg.Match(datagram.Body).Success)
                {
                    var ipMatch = ipv4Reg.Match(datagram.Body);
                    if (ipMatch.Success)
                    {
                        //Console.WriteLine(ipMatch.Groups[1].Value);   // 223.167.61.214
                        OnMessage?.Invoke(this, new IPMeesageEventArgs(ipMatch.Groups[1].Value, IpStyle.IPV4));
                    }
                }

                ////ipv6 capture
                //if (ipv6Reg != null && ipv6Reg.Match(datagram.Body).Success)
                //{
                //    var ipMatch = ipv6Reg.Match(datagram.Body);
                //    if (ipMatch.Success)
                //    {
                //        Console.WriteLine(ipMatch.Groups[1].Value);   // 223.167.61.214
                //        OnMessage?.Invoke(this, new IPMeesageEventArgs(ipMatch.Groups[1].Value, IpStyle.IPV6));
                //    }
                //}
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                break;
            }
        }
    }

    private static UdpDatagram Parse(byte[] buffer, IPEndPoint remote)
    {
        string text = Encoding.UTF8.GetString(buffer);  
        return new UdpDatagram(0, text, remote);
    }

    public async ValueTask DisposeAsync()
    {
        udp.Close();
        await receiveTask;
        udp.Dispose();
    }
}


