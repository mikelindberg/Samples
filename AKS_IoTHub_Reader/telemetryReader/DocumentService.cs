using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using telemetryReader.Models;


namespace telemetryReader
{
    public class DocumentService : IDocumentService
    {
        private string dbName;
        private string collectionName;
        private static DocumentClient _documentClient;

        public DocumentService(string endpoint, string key, string dbName, string collectionName)
        {
            this.dbName = dbName;
            this.collectionName = collectionName;
            _documentClient = new DocumentClient(new Uri(endpoint), key,
                    new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        RequestTimeout = new TimeSpan(0, 30, 0),
                        RetryOptions = new RetryOptions
                        {
                            MaxRetryAttemptsOnThrottledRequests = 10,
                            MaxRetryWaitTimeInSeconds = 30
                        }
                    });

            _documentClient.OpenAsync().GetAwaiter().GetResult();

        }

        //Store the document in a Cosmos DB collection
        public async Task StoreDocument(EventData eventData)
        {
            
            var payload = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

            //parse the data and insert depending on message schema type / if it's not a truck we discard the message
            TruckDeviceEvent parsed = JsonConvert.DeserializeObject<TruckDeviceEvent>(payload);

            Console.WriteLine("EventData contains key ..." + eventData.Properties.ContainsKey("$$MessageSchema").ToString());
            
            if (eventData.Properties.ContainsKey("$$MessageSchema") && eventData.Properties["$$MessageSchema"].ToString().ToLower() == "truck-sensors;v1")
            {
                Console.WriteLine("Parsing Truck Document...");

                var newDocument = new
                {
                    //id = eventData.SystemProperties["iothub-connection-device-id"].ToString(),
                    processedUtcTime = DateTime.UtcNow,
                    enqueudUtcTime = eventData.SystemProperties.EnqueuedTimeUtc,
                    partition = eventData.SystemProperties.PartitionKey,
                    messageOffset = eventData.Body.Offset,
                    deviceid = eventData.SystemProperties["iothub-connection-device-id"].ToString(),
                    devicetype = "truck",
                    location = new Point(parsed.longitude, parsed.latitude),
                    temperature = parsed.temperature
                };

                try
                {
                    Console.WriteLine("Inserting new doc...");
                    //Upsert instead?
                    await _documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(dbName, collectionName), newDocument);
                }
                catch (Exception e)
                {
                    if (e is DocumentClientException docEx)
                    {
                        Console.WriteLine($"ERROR with document creation - Statuc code {docEx.StatusCode} - {docEx.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"ERROR CosmosDB - {e.Message}");
                    }
                }
            }



        }
    }
}