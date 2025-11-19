namespace Aicrosoft.DDNSJob.Options;

public class DDNSOption
{
    public const string SectionName = "DDNS";

    /// <summary>
    /// 监视路由变更
    /// </summary>
    public bool EnableWatchRouteIp { get; set; }

    /// <summary>
    /// 定时检测
    /// </summary>
    public bool EnableIntervalDetectIp { get; set; }

    /// <summary>
    /// 哪种类型的DDNS
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 间隔多少秒运行一次
    /// </summary>
    public int Interval { get; set; } = 30;

    /// <summary>
    /// 登录或授权的TOken
    /// </summary>
    public string? LoginToken { get; set; }

    /// <summary>
    /// DNS解析IP
    /// </summary>
    public string? Resolver { get; set; }

    /// <summary>
    /// 取得IP地址的查询URL
    /// </summary>
    public string[]? IPUrls { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IpStyle IpStyle { get; set; }

    public string? Socks5Proxy { get; set; }

    public string? HttpProxy { get; set; }

    /// <summary>
    /// 要更新的域名列表
    /// </summary>
    public Domain[]? Domains { get; set; }

    /// <summary>
    /// 路由事件相关的配置
    /// </summary>
    public RouteEvent RouteEventSetting { get; set; }

}


public struct RouteEvent
{
    public RouteEvent()
    {
    }

    public int UpdServicePort { get; set; } = 600;

    public string? HitRegPartten { get; set; }


    public string? IpV4RegPartten { get; set; }


    public string? IpV6RegPartten { get; set; }


}

public struct Domain
{
    /// <summary>
    /// 域名： aicrosoft.com
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 主机名称
    /// </summary>
    public string[]? SubNames { get; set; }
}


public enum IpStyle
{
    IPV4 = 4,

    IPV6 = 6,
}
