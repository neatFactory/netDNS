using Aicrosoft.Validation;


namespace Aicrosoft.DDNSJob;

/// <summary>
/// Ip查找器
/// </summary>
/// <remarks>
/// 查找当前服务所在环境的外网IP地址。
/// </remarks>
public interface IIpFinder
{
    /// <summary>
    /// 取得IP地址(可能是IPV6)
    /// </summary>
    /// <returns></returns>
    public Task<IpResult?> GetIp();
}


public struct IpResult
{
    public string Ip { get; set; }

    public IpStyle IpStyle { get; set; }
}


public class IpFinder(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IIpFinder, ITransient
{
    private readonly DDNSOption config = serviceProvider.GetRequiredService<IOptions<DDNSOption>>().Value;
    private readonly Regex regV4 = new Regex(RegexLib.IPV4, RegexOptions.Compiled);
    private readonly Regex regV6 = new Regex(RegexLib.IPV6, RegexOptions.Compiled);

    public async Task<IpResult?> GetIp()
    {
        if (config.IPUrls == null || config.IPUrls.Length == 0) throw new ArgumentException($"Can not find IpUrls in DDNS.json.");
        foreach (var url in config.IPUrls)
        {
            var ip = await GetIpFrom(url);
            if (ip != null)
            {
                return ip;
            }
        }
        Logger.LogWarning($"Find all ipUrl and can not get the ip. You must check the ipurls in DDNS.json .");
        return null;
    }

    //public bool IsIpV4(string? ip)
    //{
    //    if (string.IsNullOrEmpty(ip)) return false;
    //    return regV4.IsMatch(ip);
    //}

    //public bool IsIpV6(string? ip)
    //{
    //    if (string.IsNullOrEmpty(ip)) return false;
    //    return regV6.IsMatch(ip);
    //}

    private async Task<IpResult?> GetIpFrom(string url)
    {
        using var client = new HttpClient();
        try
        {
            var response = await client.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();
            var match = regV4.Match(responseBody);
            if (match.Captures.Count > 0)
            {
                var ipv4 = match.Value;
                var val = new IpResult { Ip = ipv4, IpStyle = IpStyle.IPV4 };
                return val;
            }
            else
            {
                var matchV6 = regV6.Match(responseBody);
                if (matchV6.Captures.Count > 0)
                {
                    var ipv6 = matchV6.Value;
                    var val = new IpResult { Ip = ipv6, IpStyle = IpStyle.IPV6 };
                    return val;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Get IpAddress from {url} is failed.");
            return null;
        }
    }

}

