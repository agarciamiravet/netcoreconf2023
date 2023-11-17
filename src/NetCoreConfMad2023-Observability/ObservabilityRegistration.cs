using BuildingBlocks.Observability.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCoreConfMad2023.Observability.Extensions;
using NetCoreConfMad2023.Observability.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BuildingBlocks.Observability;

public static class ObservabilityRegistration
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        // This is required if the collector doesn't expose an https endpoint. By default, .NET
        // only allows http2 (required for gRPC) to secure endpoints.
        //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var configuration = builder.Configuration;

        ObservabilityOptions observabilityOptions = new();

        configuration
            .GetRequiredSection(nameof(ObservabilityOptions))
            .Bind(observabilityOptions);

        builder.Host.AddSerilog();

        builder.Services
                .AddOpenTelemetry()
                .AddTracing(observabilityOptions)
                .AddMetrics(observabilityOptions);

        return builder;
    }

    public static WebApplicationBuilder AddObservabilityApi(this WebApplicationBuilder builder)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        // This is required if the collector doesn't expose an https endpoint. By default, .NET
        // only allows http2 (required for gRPC) to secure endpoints.
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var configuration = builder.Configuration;

        ObservabilityOptions observabilityOptions = new();

        configuration
            .GetRequiredSection(nameof(ObservabilityOptions))
            .Bind(observabilityOptions);

        builder.Host.AddSerilog();

        builder.Services
                .AddOpenTelemetry()
                .AddTracingAPI(observabilityOptions)
                .AddMetrics(observabilityOptions);

        return builder;
    }

    private static OpenTelemetryBuilder AddTracingAPI(this OpenTelemetryBuilder builder, ObservabilityOptions observabilityOptions)
    {


        builder.WithTracing(tracing =>
        {
            tracing
                .AddSource(observabilityOptions.ServiceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(observabilityOptions.ServiceName))
                .SetErrorStatusOnException()
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(options =>
                {
                    //options.EnableGrpcAspNetCoreSupport = true;
                    options.RecordException = true;
                })
                .AddHttpClientInstrumentation()                
                .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.EnrichWithIDbCommand = (activity, command) =>
                             {
                                 var stateDisplayName = $"{command.CommandType} main";
                                 activity.DisplayName = stateDisplayName;
                                 activity.SetTag("db.name", stateDisplayName);
                                 activity.SetTag("db.query", command.CommandText);
                             };
                        })
                        .AddRedisInstrumentation()
                       .ConfigureRedisInstrumentation((sp, i) =>
                        {
                            var cache = (RedisCache)sp.GetRequiredService<IDistributedCache>();
                            var conn = cache.GetConnection();
                            i.AddConnection(cache.GetConnection());
                        });

            tracing
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = observabilityOptions.CollectorUri;
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        });

        return builder;
    }

    private static OpenTelemetryBuilder AddTracing(this OpenTelemetryBuilder builder, ObservabilityOptions observabilityOptions)
    {
        builder.WithTracing(tracing =>
        {
            tracing
                .AddSource(observabilityOptions.ServiceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(observabilityOptions.ServiceName))
                .SetErrorStatusOnException()
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.EnableGrpcAspNetCoreSupport = true;
                    options.RecordException = true;
                });

            tracing
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = observabilityOptions.CollectorUri;
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        });

        return builder;
    }

    private static OpenTelemetryBuilder AddMetrics(this OpenTelemetryBuilder builder, ObservabilityOptions observabilityOptions)
    {
        var meters = new NetCoreConf2023Metrics();

        builder.WithMetrics(metrics =>
        {           
            var meter = new Meter(observabilityOptions.ServiceName);

            metrics
                .AddMeter(meter.Name)
                 .AddMeter(meters.MetricName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(meter.Name))
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddAspNetCoreInstrumentation();
            metrics
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = observabilityOptions.CollectorUri;
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        });

        builder.Services.AddSingleton<NetCoreConf2023Metrics>(meters);

        return builder;
    }

    public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder
            .UseSerilog((context, provider, options) =>
            {
                var environment = context.HostingEnvironment.EnvironmentName;
                var configuration = context.Configuration;

                ObservabilityOptions observabilityOptions = new();

                configuration
                    .GetSection(nameof(ObservabilityOptions))
                    .Bind(observabilityOptions);

                var serilogSection = $"{nameof(ObservabilityOptions)}:{nameof(ObservabilityOptions)}:Serilog";

                var config = context.Configuration.GetRequiredSection("ObservabilityOptions:Serilog");

                options
                    .ReadFrom.Configuration(config)
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentVariable(environment)
                    .Enrich.WithProperty("ApplicationName", observabilityOptions.ServiceName)
                    .WriteTo.Console();

                options.WriteTo.OpenTelemetry(cfg =>
                {
                    cfg.Endpoint = $"{observabilityOptions.CollectorUrl}/v1/logs";
                    cfg.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField;
                    cfg.ResourceAttributes = new Dictionary<string, object>
                                                {
                                                    {"service.name", observabilityOptions.ServiceName},
                                                    {"index", 10},
                                                    {"flag", true},
                                                    {"value", 3.14}
                                                };
                });

            });
        return hostBuilder;
    }

}
