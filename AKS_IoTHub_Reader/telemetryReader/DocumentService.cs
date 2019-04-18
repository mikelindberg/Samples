using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.EventHubs;

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
                    new ConnectionPolicy{
                        ConnectionMode = ConnectionMode.Direct,
                        RequestTimeout = new TimeSpan(1, 0, 0),
                        RetryOptions = new RetryOptions{
                            MaxRetryAttemptsOnThrottledRequests = 10,
                            MaxRetryWaitTimeInSeconds = 30
                        }
                    });

            _documentClient.OpenAsync().GetAwaiter().GetResult();

        }

        //Store the document in a Cosmos DB collection
        public async Task StoreDocument(EventData eventData)
        {
            Console.WriteLine("Storing Document...");
            var payload = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            
            var newDocument = new
            {
                processedUtcTime = DateTime.UtcNow,
                enqueudUtcTime = eventData.SystemProperties.EnqueuedTimeUtc,
                partition = eventData.SystemProperties.PartitionKey,
                messageOffset = eventData.Body.Offset,
                deviceid = eventData.SystemProperties["iothub-connection-device-id"].ToString(),
                payload = payload
            };

            try{
                await _documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(dbName, collectionName), newDocument);
            }
            catch(Exception e)
            {
                if(e is DocumentClientException docEx)
                {
                    Console.WriteLine($"ERROR with document creation - Statuc code {docEx.StatusCode} - {docEx.Message}");
                }else
                {
                    Console.WriteLine($"ERROR CosmosDB - {e.Message}");
                }
            }



            
        }
    }
}