using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Serilog;

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

    policyBuilder
        .WithOrigins(corsConfig.AllowedOrigins)
        .WithMethods("POST")
        .WithHeaders("Content-Type");
});

app.UseSerilogIngestion();

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
        Optional = false,
        ReloadOnChange = true
    };
    sources.Add(jsonSource);
}

internal class CorsConfig
{
    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();
}