namespace Aicrosoft;

public sealed class JobAppSetup : PluginSetupBase
{

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        base.ConfigureServices(hostBuilderContext, services);

        var config = hostBuilderContext.Configuration;
        services.Configure<DDNSOption>(config.GetSection(DDNSOption.SectionName));

        //var setting = config.GetSection(DDNSOption.SectionName).Get<DDNSOption>();
        //$"setting1-->{setting.ToJson()}".ToWarn();

        //var connStr = hostBuilderContext.Configuration.GetValue<string>("DDNS:loginToken");
        //$"setting2-->{connStr}".ToWarn();
    }

}
