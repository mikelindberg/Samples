// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
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
            InitializeConfiguration();
            var runAsFunction = Boolean.Parse( Configuration["runAsFunction"]);
            if(runAsFunction == true) await RunAsFunction();
            else await RunAsEPHProcessor();
        }

        static async Task RunAsFunction()
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

        private static IConfigurationRoot Configuration { get; set; }

        private static void InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.development.json", false, true);
            Configuration = builder.Build();
        }

        static async Task RunAsEPHProcessor()
        {


            //Event hub settings
            string EventHubName = Configuration["EventHubName"];
            string EventHubConnectionString = Configuration["ConnectionStrings:TestEventHubConnection"];

            //Storage settings
            string StorageContainerName = Configuration["StorageContainerName"];
            string StorageConnectionString = Configuration["ConnectionStrings:AzureWebJobsStorage"];

            //Setup event processor host

            var eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);


            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();


            Console.WriteLine("Receiving. Press ENTER to stop worker.");

            Console.ReadLine();

            // Disposes of the Event Processor Host
            //await eventProcessorHost.UnregisterEventProcessorAsync();


        }
    }
}
