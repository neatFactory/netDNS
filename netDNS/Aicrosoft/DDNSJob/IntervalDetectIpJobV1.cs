namespace Aicrosoft.DDNSJob;

public sealed class IntervalDetectIpJobV1 : Job
{
    public IntervalDetectIpJobV1(IServiceProvider serviceProvider)
    {
        var setting = serviceProvider.GetRequiredService<IOptions<DDNSOption>>().Value;
        //Timeout = 10000;
        Enable = setting.EnableIntervalDetectIp;
        Trigger = TimeSpan.FromSeconds(setting.Interval).ToString();

    }

    public override string? WorkerName => nameof(DDNSWorker);

}




