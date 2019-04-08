// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;

namespace SampleHost
{
    class Program
    {
        public static async Task Main(string[] args)
        {


            string globalAppInsightsKey = String.Empty;
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices()
                    .AddEventHubs(a =>
                    {
                        a.BatchCheckpointFrequency = 10;
                        a.EventProcessorOptions.MaxBatchSize = 1;
                        a.EventProcessorOptions.PrefetchCount = 10;
                    });
                })
                .ConfigureAppConfiguration(b =>
                {
                    b.AddJsonFile("appsettings.development.json");
                })
                .ConfigureLogging((context, b) =>
                {
                    b.SetMinimumLevel(LogLevel.Trace);

                    b.AddAzureWebAppDiagnostics();
                    b.AddConsole();

                    string appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                    globalAppInsightsKey = appInsightsKey;
                    // If this key exists in any config, use it to enable App Insights
                    if (!string.IsNullOrEmpty(appInsightsKey))
                    {
                        b.AddApplicationInsights(o => o.InstrumentationKey = appInsightsKey);
                    }
                })
                //.ConfigureServices(serviceCollection => serviceCollection
                //.Configure<AzureFileLoggerOptions>(options =>
                //{
                //    options.FileName = "azure-diagnostics-";
                //    options.FileSizeLimit = 50 * 1024;
                //    options.RetainedFileCountLimit = 5;
                //}).Configure<AzureBlobLoggerOptions>(options =>
                //{
                //    options.BlobName = "log.txt";
                //}))
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {

                var config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(globalAppInsightsKey);
                var telemetry = new Microsoft.ApplicationInsights.TelemetryClient(config);
                telemetry.TrackTrace("WebJob host initialized and starting...");

                await host.RunAsync();


            }
        }
    }
}
