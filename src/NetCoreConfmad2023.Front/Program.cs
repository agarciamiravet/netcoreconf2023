using BuildingBlocks.Observability.Options;
using NetCoreConfmad2023.Front.Data;
using NetCoreConfmad2023.Front.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

ObservabilityOptions observabilityOptions = new();

builder.Services.AddHealthChecks();

// Add HttpClient
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CartService>();


configuration
    .GetRequiredSection(nameof(ObservabilityOptions))
    .Bind(observabilityOptions);

builder.Services.AddOpenTelemetry().WithTracing(builder =>
{
    builder.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(observabilityOptions.ServiceName))
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(observabilityOptions.CollectorUrl);
        });
});

var app = builder.Build();

app.MapHealthChecks("/healthz");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
