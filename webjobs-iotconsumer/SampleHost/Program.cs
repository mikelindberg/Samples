// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs.EventHubs;
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
            await RunAsFunction();
        }

        static async Task RunAsFunction()
        {

            string globalAppInsightsKey = String.Empty;
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices();
                    b.AddEventHubs(a =>
                        a.EventProcessorOptions.ReceiveTimeout = new TimeSpan(0, 0, 3) // default is 1 min. Is that maybe one of problems?
                    );
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
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(globalAppInsightsKey);
                var telemetry = new Microsoft.ApplicationInsights.TelemetryClient(config);
                telemetry.TrackTrace("WebJob host initialized and starting...");

                foreach(var item in assemblies)
                {
                    telemetry.TrackTrace($"Loaded assembly: {item.FullName}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);
                }
                await host.RunAsync();
            }
        }

    }
}
