using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.DataContracts;

namespace TestHostEPH
{
    class Program
    {

        private static Microsoft.ApplicationInsights.TelemetryClient telemetry = null;
        private static IConfigurationRoot Configuration { get; set; }

        public static async Task Main(string[] args)
        {
            await RunAsEPHProcessor();
        }


        private static void InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.development.json", false, true);
            Configuration = builder.Build();

            var appinsightKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            var config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(appinsightKey);
            telemetry = new Microsoft.ApplicationInsights.TelemetryClient(config);
            telemetry.TrackTrace("EPH configuration initialized...", SeverityLevel.Information);
        }

        static async Task RunAsEPHProcessor()
        {
            EventProcessorHost eventProcessorHost = null;
            try
            {
                InitializeConfiguration();

                //Event hub settings
                string EventHubName = Configuration["EventHubName"];
                string EventHubConnectionString = Configuration["ConnectionStrings:TestEventHubConnection"];

                //Storage settings
                string StorageContainerName = Configuration["StorageContainerName"];
                string StorageConnectionString = Configuration["ConnectionStrings:AzureWebJobsStorage"];

                //Setup event processor host

                eventProcessorHost = new EventProcessorHost(
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

                // Registers the Event Processor Host and starts receiving messages
                await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();


                Console.WriteLine("Receiving. Press ENTER to stop worker.");

                Console.ReadLine();

                // Disposes of the Event Processor Host

            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw ex;
            }
            finally
            {
                if (eventProcessorHost != null) await eventProcessorHost.UnregisterEventProcessorAsync();
            }


        }
    }
}
