namespace BuildingBlocks.Observability.Options;

public class ObservabilityOptions
{
    public string ServiceName { get; set; } = default!;
    public string CollectorUrl { get; set; } = @"http://localhost:4317";
    public string JaegerUrl { get; set; } = @"http://localhost:4327";

    public Uri CollectorUri => new(this.CollectorUrl);

    public Uri JaegerUri => new(this.JaegerUrl);
}
