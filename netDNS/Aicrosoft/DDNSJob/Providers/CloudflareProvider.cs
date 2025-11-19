namespace Aicrosoft.DDNSJob.Providers;

[KeyedName("Cloudflare")]
public sealed class CloudflareProvider(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IDDNSProvider, IKeyedTransient
{

    private readonly DDNSOption config = serviceProvider.GetRequiredService<IOptions<DDNSOption>>().Value;


    public async Task<bool> UpdateDomains(DDNSState state)
    {
        var askStyle = state.GetUpdateIpResult();
        if (askStyle == IpResultStyle.NoNeedUpdate)
            return false; //不需要更新

        var domains = config.Domains;
        if (domains == null) return false;

        // HTTP 客户端（全局复用）
        using var http = new HttpClient
        {
            BaseAddress = new Uri("https://api.cloudflare.com/client/v4/")
        };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.LoginToken);

        var zones = await GetZonesAsync(http);
        if (zones == null || zones.Length == 0)
        {
            Logger.LogError($"Get Current Token's all zones information was failed.");
            return false;
        }

        foreach (var domain in domains)
        {
            if (domain.SubNames == null)
            {
                Logger.LogWarning($"Domain[{domain}] has no set subNames. break this domain.");
                continue;
            }
            var zone = zones.FirstOrDefault(x => x.Name == domain.Name);
            if (zone == null)
            {
                Logger.LogWarning($"Get Domain[{domain}]'s zone is falied. break this domain.");
                continue;
            }
            var rs = await GetAllSubNamesAsync(http, zone.Id);
            if (rs == null)
            {
                Logger.LogWarning($"Get all subNames recored is falied. break this domain.");
                continue;
            }

            foreach (string? subName in domain.SubNames)
            {
                if (subName == null) continue;
                var rd = rs.FirstOrDefault(x => x.Name == $"{subName}.{domain.Name}");
                if (rd == null)
                {
                    //add new sub record.
                    var val = await CreateRecordAsync(http, zone.Id, subName, state);
                }
                else
                {
                    //update sub record.
                    if (rd.Content == state.QueryIpV4)
                    {
                        Logger.LogDebug($"[{rd.Name}] same IP address, do not need update.");
                        continue;
                    }
                    var val = await UpdateRecordAsync(http, zone.Id, rd, state);
                }
            }
        }

        return true;
    }


    private async Task<CFRecordDTO[]?> GetAllSubNamesAsync(HttpClient http, string zoneId)
    {
        Logger.LogDebug($"Begin query current token's all subNames information.");
        var url = $"zones/{zoneId}/dns_records?per_page=1000";
        var resp = await http.GetFromJsonAsync<CFResponseDTO<CFRecordDTO[]>>(url);
        Logger.LogTrace($"GetAllSubNamesAsync result --> {resp.ToJson()}");
        return resp?.Result;
    }

    /// <summary>
    /// query current token's all zones information
    /// </summary>
    /// <param name="http"></param>
    /// <returns></returns>
    private async Task<CFZoneDTO[]?> GetZonesAsync(HttpClient http)
    {
        Logger.LogDebug($"Begin query current token's all zones information.");
        //var response = await http.GetAsync($"zones?name={baseDomain}");
        var resp = await http.GetFromJsonAsync<CFResponseDTO<CFZoneDTO[]>>($"zones");
        Logger.LogTrace($"GetZonesAsync result --> {resp.ToJson()}");
        return resp?.Result;
    }


    private async Task<CFRecordDTO?> CreateRecordAsync(HttpClient http, string zoneId, string subName, DDNSState state)
    {
        if (state.QueryIpV4 == null) return null;
        var payload = new
        {
            name = subName, // 子域名，如 "api" 或 "home"
            ttl = 1, // auto
            type = "A", //only for ipV4 , ipv6 is 'AAAA'
            content = state.QueryIpV4, // 要指向的 IPv4
            proxied = false // true = 橙色云朵代理
        };
        using var json = JsonContent.Create(payload);
        var response = await http.PostAsync($"zones/{zoneId}/dns_records", json);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CFResponseDTO<CFRecordDTO>>();
        Logger.LogDebug($"CreateRecordAsync result --> {created.ToJson()}");
        return created!.Result!;
    }


    private async Task<CFRecordDTO?> UpdateRecordAsync(HttpClient http, string zoneId, CFRecordDTO rd, DDNSState state)
    {
        if (state.QueryIpV4 == null || rd.Content == state.QueryIpV4) return null;
        var payload = new
        {
            id = rd.Id,
            type = rd.Type,
            name = rd.Name,
            content = state.QueryIpV4,
            ttl = rd.Ttl,
            proxied = rd.Proxied,
        };
        using var json = JsonContent.Create(payload);
        var resp = await http.PutAsync($"zones/{zoneId}/dns_records/{rd.Id}", json);
        resp.EnsureSuccessStatusCode();

        var updated = await resp.Content.ReadFromJsonAsync<CFResponseDTO<CFRecordDTO>>();
        Logger.LogDebug($"UpdateRecordAsync result --> {updated.ToJson()}");
        return updated?.Result;
    }



    #region CloudFlare Response DTOs


    record class CFResponseDTO<T>(bool Success, T Result, object[]? Errors = null);

    record class CFZoneDTO(string Id, string Name, string Status);

    record class CFRecordDTO(string Id, string Name, string Type, string Content, int Ttl, bool Proxied);


    #endregion


}



