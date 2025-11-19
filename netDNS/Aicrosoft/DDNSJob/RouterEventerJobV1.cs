
namespace Aicrosoft.DDNSJob;

public sealed class RouterEventerJobV1 : Job
{
    public RouterEventerJobV1(IServiceProvider serviceProvider)
    {
        var setting = serviceProvider.GetRequiredService<IOptions<DDNSOption>>().Value;
        //Timeout = 10000;
        Enable = setting.EnableWatchRouteIp;
        Trigger = nameof(RouterEventer);
    }

    public override string? WorkerName => nameof(DDNSWorker);

}

