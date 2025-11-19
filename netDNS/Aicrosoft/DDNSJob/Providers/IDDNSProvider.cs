namespace Aicrosoft.DDNSJob.Providers;

/// <summary>
/// DDNS Provider
/// </summary>
public interface IDDNSProvider
{
    /// <summary>
    /// 更新所有的域名
    /// </summary>
    /// <returns>
    /// 更新成功后可把JobContext中的内容进行交换
    /// </returns>
    Task<bool> UpdateDomains(DDNSState state);

}
