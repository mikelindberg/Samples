using System;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Runtime.Loader;

namespace telemetryReader
{
    class Program
    {
        #region secrets
        //All secrets are configured with Secret Manager Tool! 
        //You can see each secret setup above the secret it self!

        //dotnet user-secrets set event-hub-connectionstring "{Event Hubs connection string}"
        // private static string EventHubConnectionStringSecret = "event-hub-connectionstring";

        // //dotnet user-secrets set storage-account-key "{Storage account key}"
        // private static string StorageAccountConnectionStringSecret = "storage-account-key";
        #endregion

        public static ManualResetEvent _Shutdown = new ManualResetEvent(false);
        public static ManualResetEventSlim _Complete = new ManualResetEventSlim();

        private static IConfigurationRoot Configuration { get; set; }
        private static EventProcessorHost eventProcessorHost;

        static int Main(string[] args)
        {
            try
            {
                var ended = new ManualResetEventSlim();
                var starting = new ManualResetEventSlim();

                Console.Write("Starting application...");
                InitializeConfiguration();
                MainAsync(args).GetAwaiter().GetResult();

                // Wait for a singnal
                _Shutdown.WaitOne();
                Console.WriteLine("Shutting down...");
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                Console.Write("Cleaning up resources");
            }

            Console.Write("Exiting...");
            _Complete.Set();

            return 0;
        }

        //Load secrets and appsettings.json
        private static void InitializeConfiguration()
        {
            string settingsFile = "appsettings.json";
#if DEBUG
            settingsFile = System.IO.Directory.GetCurrentDirectory() + "/appsettings.development.json";
#endif

            Console.WriteLine(settingsFile);
            var builder = new ConfigurationBuilder()
                .AddJsonFile(settingsFile, false, true)
                .AddUserSecrets<Program>();

            Configuration = builder.Build();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Registering EventProcessor...");

            //Event hub settings
            string EventHubName = Configuration["eventhub:eventHubName"];
            string EventHubConnectionString = Configuration["eventhub:eventHubConnectionString"];

            Console.WriteLine("Eventhub: " + EventHubName);
            Console.WriteLine("Eventhub connectionstring: " + EventHubConnectionString);

            //Storage settings
            string StorageContainerName = Configuration["eventhub:storageContainerName"];
            string StorageConnectionString = Configuration["storage:storageConnectionString"];

            Console.WriteLine("Storage container: " + StorageContainerName);
            Console.WriteLine("Storage connection string: " + StorageConnectionString);

            //Setup event processor host
            eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);

            IDocumentService documentService = new DocumentService(
                Configuration["storage:storageConnectionString"],
                Configuration["storage:storageContainerName"]
            );

            IEventProcessorFactory processorFactory = new SimpleEventProcessorFactory(documentService);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorFactoryAsync(processorFactory);
        }

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            Console.Write($"Shutting down in response to SIGTERM.");
            Task t = eventProcessorHost.UnregisterEventProcessorAsync();
            t.Wait();
            _Shutdown.Set();
            _Complete.Wait();
        }
    }
}