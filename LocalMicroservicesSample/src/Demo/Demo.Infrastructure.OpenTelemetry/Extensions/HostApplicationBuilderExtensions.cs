using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;

namespace Demo.Infrastructure.OpenTelemetry.Extensions
{
    public static class HostApplicationBuilderExtensions
    {
        public static OpenTelemetryBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
        {
            // See https://github.com/open-telemetry/opentelemetry-dotnet
            // See https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/samples/Diagnostics.md#enabling-experimental-tracing-features

            AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

            var resourceBuilder = ConfigureResourceBuilder(ResourceBuilder.CreateDefault(), builder.Environment);

            
            builder.Logging.AddOpenTelemetry(otelLoggerOptions =>
            {
                otelLoggerOptions.IncludeScopes = true;
                otelLoggerOptions.IncludeFormattedMessage = true;
                otelLoggerOptions
                    .SetResourceBuilder(resourceBuilder)
                    .AddOtlpExporter();
            });

            var otel = builder.Services.AddOpenTelemetry().WithTracing(otelTraceBuilder =>
            {
                otelTraceBuilder
                   .SetResourceBuilder(resourceBuilder)
                   .AddSource("Azure.*")            // Collect all traces from Azure SDKs
                   .AddSource("Demo.*")
                   .AddHttpClientInstrumentation(options =>
                   {
                       options.RecordException = true;

                       // See https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/samples/Diagnostics.md#filtering-out-duplicated-http-client-activities
                       options.FilterHttpRequestMessage = (_) => Activity.Current?.Parent?.Source.Name != "Azure.Core.Http";
                   })
                   .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                   .AddSqlClientInstrumentation(options =>
                   {
                       options.SetDbStatementForText = true;
                       options.RecordException = true;
                   })
                   .AddOtlpExporter();
            });

            otel.WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddOtlpExporter());

            return otel;
        }

        private static ResourceBuilder ConfigureResourceBuilder(ResourceBuilder resource, IHostEnvironment hostingEnvironment)
        {
            return resource.AddService(serviceName: hostingEnvironment.ApplicationName, serviceInstanceId: Environment.MachineName);
        }
    }
}
