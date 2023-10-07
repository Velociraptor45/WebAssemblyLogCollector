# WebAssemblyLogCollector

The WebAssemblyLogCollector wraps the `Serilog.AspNetCore.Ingestion` package in order to provide a standalone application that collects logs for e.g. Blazor WebAssembly applications.
For further information on how to send logs to the log collector, see [Serilog.Sinks.BrowserHttp](https://github.com/nblumhardt/serilog-sinks-browserhttp).

## Configuration
You can create a custom `appsettings.json` file and place it in the volume mapped to `/app/config` (see Docker/compose-file.yml). By default Kestrel is configured to port 80 and Cors allows all origins. You should at least overwrite the latter.</br>
If you leave out the whole `Ingestion` section or individual properties in it, the default values from `Serilog.AspNetCore.Ingestion` will be used.</br>
For Serilog logging, the **File** and **Console** sinks are currently available.

Example:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      }
    }
  },
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:5000"
    ]
  },
  "Ingestion": {
    "EndpointPath": "/my-endpoint",
    "OriginPropertyName": "MyProp",
    "EventBodyLimitBytes": 20437234,
    "MinLogLevel": "Information"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": ".\\logs\\logs.log",
          "rollOnFileSizeLimit": true,
          "fileSizeLimitBytes": 1048576,
          "retainedFileCountLimit": 20
        }
      }
    ]
  }
}
```