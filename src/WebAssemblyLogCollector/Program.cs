using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Core;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

AddAppsettingsSourceTo(builder.Configuration.Sources);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    var corsConfig = app.Configuration.GetSection("Cors").Get<CorsConfig>();

    if (corsConfig is null)
        throw new InvalidOperationException("Cors section is missing in appsettings.json");

    var allowedOrigins = corsConfig.AllowedOrigins.Any() ? corsConfig.AllowedOrigins : new[] { "*" };

    policyBuilder
        .WithOrigins(allowedOrigins)
        .WithMethods("POST")
        .WithHeaders("Content-Type");
});

var ingestionConfig = app.Configuration.GetSection("Ingestion").Get<IngestionConfig>();

app.UseSerilogIngestion(opt =>
{
    if (ingestionConfig is null)
        return;

    if (ingestionConfig.EndpointPath is not null)
        opt.EndpointPath = ingestionConfig.EndpointPath;

    if (ingestionConfig.OriginPropertyName is not null)
        opt.OriginPropertyName = ingestionConfig.OriginPropertyName;

    if (ingestionConfig.EventBodyLimitBytes is not null)
        opt.EventBodyLimitBytes = ingestionConfig.EventBodyLimitBytes.Value;

    if (ingestionConfig.MinLogLevel is not null)
        opt.ClientLevelSwitch = new LoggingLevelSwitch(ingestionConfig.MinLogLevel.Value);
});

await app.RunAsync();

static void AddAppsettingsSourceTo(IList<IConfigurationSource> sources)
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var basePath = env == "Local"
        ? Directory.GetCurrentDirectory()
        : Path.Combine(Directory.GetCurrentDirectory(), "config");
    var jsonSource = new JsonConfigurationSource
    {
        FileProvider = new PhysicalFileProvider(basePath),
        Path = "appsettings.json",
        Optional = true,
        ReloadOnChange = true
    };
    sources.Add(jsonSource);
}

internal class CorsConfig
{
    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();
}

internal class IngestionConfig
{
    public string? EndpointPath { get; init; }
    public string? OriginPropertyName { get; init; }
    public long? EventBodyLimitBytes { get; init; }
    public LogEventLevel? MinLogLevel { get; init; }
}