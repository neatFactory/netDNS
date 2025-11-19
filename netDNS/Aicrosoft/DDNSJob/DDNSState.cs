namespace Aicrosoft.DDNSJob;

public class DDNSState
{
    /// <summary>
    /// 当前查询到的IPV4
    /// </summary>
    public string? QueryIpV4 { get; set; }

    /// <summary>
    /// 当前查询到的IPV6
    /// </summary>
    public string? QueryIpV6 { get; set; }

    /// <summary>
    /// 最后更新的IP地址V4
    /// </summary>
    public string? UpdatedIpV4 { get; set; }

    /// <summary>
    /// 最后更新的IP地址V6
    /// </summary>
    public string? UpdatedIpV6 { get; set; }

    /// <summary>
    /// 空时不需要更新
    /// </summary>
    /// <returns></returns>
    public IpResultStyle GetUpdateIpResult()
    {
        IpResultStyle rst = IpResultStyle.NeedQuery;

        if ((!string.IsNullOrWhiteSpace(QueryIpV4) && QueryIpV4 == UpdatedIpV4) || (!string.IsNullOrWhiteSpace(QueryIpV6) && QueryIpV6 == UpdatedIpV6))
        {
            rst = IpResultStyle.NoNeedUpdate;
            return rst;
        }

        if (!string.IsNullOrWhiteSpace(QueryIpV4) && QueryIpV4 != UpdatedIpV4)
            rst = IpResultStyle.NeedUpdate;
        if (!string.IsNullOrWhiteSpace(QueryIpV6) && QueryIpV6 != UpdatedIpV6)
            rst = IpResultStyle.NeedUpdate;

        return rst;
    }

    public bool SetQueryIp(IpResult? ip)
    {
        if (ip == null) return false;

        if (ip.Value.IpStyle == IpStyle.IPV4)
        {
            QueryIpV4 = ip.Value.Ip;
            return true;
        }

        if (ip.Value.IpStyle == IpStyle.IPV6)
        {
            QueryIpV6 = ip.Value.Ip;
            return true;
        }

        return false;
    }
}

public enum IpResultStyle
{

    NoNeedUpdate = 1,

    NeedQuery = 2,

    NeedUpdate = 3,

}

