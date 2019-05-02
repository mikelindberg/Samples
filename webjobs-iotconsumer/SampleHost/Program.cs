// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.EventHubs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Options;

namespace SampleHost
{
    class Program
    {
        private static Microsoft.ApplicationInsights.TelemetryClient telemetry = null;
        private static IConfigurationRoot Configuration { get; set; }

        public static async Task Main(string[] args)
        {
            await RunAsFunction();
        }

        private static void InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.development.json", false, true);
            Configuration = builder.Build();

            var appinsightKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            var config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(appinsightKey);
            telemetry = new Microsoft.ApplicationInsights.TelemetryClient(config);
            telemetry.TrackTrace("EPH configuration initialized...", SeverityLevel.Information);
        }

        static async Task RunAsFunction()
        {

            InitializeConfiguration();

            //Event hub settings
            string EventHubName = Configuration["EventHubName"];
            string EventHubConnectionString = Configuration["ConnectionStrings:TestEventHubConnection"];

            //Storage settings
            string StorageContainerName = Configuration["StorageContainerName"];
            string StorageConnectionString = Configuration["ConnectionStrings:AzureWebJobsStorage"];


            string globalAppInsightsKey = String.Empty;
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    EventProcessorHost eventProcessorHost = new EventProcessorHost(
                    EventHubName,
                    PartitionReceiver.DefaultConsumerGroupName,
                    EventHubConnectionString,
                    StorageConnectionString,
                    StorageContainerName);
                    eventProcessorHost.PartitionManagerOptions = new PartitionManagerOptions()
                    {
                        LeaseDuration = new TimeSpan(0, 0, 15),
                        RenewInterval = new TimeSpan(0, 0, 4)
                    };

                    b.AddAzureStorageCoreServices();
                    b.AddEventHubs(a => a.AddEventProcessorHost(EventHubName, eventProcessorHost));
                    //b.AddAzureStorageCoreServices();
                    //b.AddEventHubs();
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

                var options = host.Services.GetService<IOptions<EventHubOptions>>().Value;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(globalAppInsightsKey);
                var telemetry = new Microsoft.ApplicationInsights.TelemetryClient(config);
                telemetry.TrackTrace("WebJob host initialized and starting...");

                foreach(var item in assemblies)
                {
                    telemetry.TrackTrace($"Loaded assembly: {item.FullName}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);
                }
                await host.RunAsync();
                await host.WaitForShutdownAsync();
            }
        }

    }
}
