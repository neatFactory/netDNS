namespace Aicrosoft.DDNSJob.Providers;

[KeyedName("DNSPod")]
public class DNSPodProvider : ServiceBase, IDDNSProvider, IKeyedTransient
{
    const string apiUri = "https://dnsapi.cn/";
    const string format = "format=json";
    private readonly DDNSOption config;
    private readonly string loginToken;

    public DNSPodProvider(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = serviceProvider.GetRequiredService<IOptions<DDNSOption>>().Value;
        loginToken = $"login_token={config.LoginToken}";
    }


    public async Task<bool> UpdateDomains(DDNSState state)
    {
        
        await Task.CompletedTask;
        return false;

    }


    private async Task<string?> HttpRequest(string apiUrl)
    {
        try
        {
            //公有参数
            var pms = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(config.LoginToken))
            {
                pms.Add("login_token", config.LoginToken);
            }
            pms.Add("format", "json");
            pms.Add("lang", "en");
            pms.Add("error_on_empty", "no");

            //api接口独有参数
            pms.Add("Content-Type", "application/x-www-form-urlencoded");
            pms.Add("User-Agent", "Aicrosoft/DDNS/0.1");

            using var client = new HttpClient();
            //client.Encoding = Encoding.UTF8;
            var response = await client.PostAsync(apiUrl, null);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            else
            {
                Logger.LogWarning($"Request {apiUri} is failed with status code:{response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Request {apiUrl} has exception.");
            return null;
        }
    }



}
