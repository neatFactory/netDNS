namespace Aicrosoft.DDNSJob;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 这里为了事件Worker和间隔运行的Worker，使用<see cref="TimeJobContext"/>来作为上下文，两个不同类型的Worker执行同一个逻辑功能。
/// </remarks>
[KeyedName]
public sealed class DDNSWorker(IServiceProvider serviceProvider) : Worker<TimeJobContext>(serviceProvider)
{
    private readonly DDNSOption config = serviceProvider.GetRequiredService<IOptions<DDNSOption>>().Value;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var state = Context!.GetData<DDNSState>();
        if (state == null)
        {
            state = new DDNSState();
            Context!.SetData(state);
        }
        var askStyle = state.GetUpdateIpResult();

        //非事件传来的，没有IP信息，就通过IPFinder找到当前的IP地址。
        if (askStyle == IpResultStyle.NeedQuery)
        {
            //没有Ip的值，要自已去请求获取
            Logger.LogDebug($"Begin query Current IP address.");
            var ipfinder = ServiceProvider.GetRequiredService<IIpFinder>();
            var ip = await ipfinder.GetIp();
            state!.SetQueryIp(ip);
            Logger.LogInformation($"Query Current IP address is {ip.ToJson()} .");
            askStyle = state.GetUpdateIpResult();
        }

        //上次更新的IP和当前的IP一致，不用更新
        if (askStyle == IpResultStyle.NoNeedUpdate)
        {
            Context.Message = $"The current IP address queried this time is the same as the last time, so no update is required.";
            Logger.LogInformation(Context.Message);
            await ExecuteCallbackAsync(false);
            return;
        }
        else
        {
            //更新IP
            var provider = ServiceProvider.GetRequiredKeyedService<IDDNSProvider>(config.Provider);
            var val = await provider.UpdateDomains(state);
            Logger.LogInformation($"UpdateDomains by{provider} result is {val} .");
            await ExecuteCallbackAsync(val);
        }
    }

    protected override async Task ExecuteCallbackAsync<TResult>(TResult? result) where TResult : default
    {
        var state = Context!.GetData<DDNSState>();
        if (state == null) return;

        if (result is true)
        {
            //更新成功
            state.UpdatedIpV4 = state.QueryIpV4;
            state.UpdatedIpV6 = state.QueryIpV6;
        }

        state.QueryIpV4 = null;
        state.QueryIpV6 = null;

        await Task.CompletedTask;
    }
}


