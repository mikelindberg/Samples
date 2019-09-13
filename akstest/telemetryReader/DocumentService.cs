using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Newtonsoft.Json;

namespace telemetryReader
{
    public class DocumentService : IDocumentService
    {
        private string storageConnectionString;
        private string storageContainerName;
        private CloudBlobContainer container;

        public DocumentService(string storageConnectionString, string storageContainerName)
        {
            this.storageConnectionString = storageConnectionString;
            this.storageContainerName = storageContainerName;

            Console.WriteLine("ConnectionString: {0}", this.storageConnectionString);
            Console.WriteLine("Container: {0}", this.storageContainerName);
        }

        public async Task CreateClient()
        {
            try
            {
                // Retrieve storage account information from connection string
                CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString();

                // Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Create a container for organizing blobs within the storage account.
                Console.WriteLine("Creating Container");
                this.container = blobClient.GetContainerReference(storageContainerName);

                // Change the retry policy for this call so that if it fails, it fails quickly.
                BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new NoRetry() };
                await container.CreateIfNotExistsAsync(requestOptions, null);
            }
            catch (StorageException ex)
            {
                Console.WriteLine("If you are running with the default connection string, please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.WriteLine(ex);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Store the document in a blob
        public async Task StoreDocument(EventData eventData)
        {
            try
            {
                if (container == null)
                    await CreateClient();

                var payload = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                //parse the data and insert depending on message schema type / if it's not a truck we discard the message
                TruckDeviceEvent parsed = JsonConvert.DeserializeObject<TruckDeviceEvent>(payload);

                if (eventData.Properties.ContainsKey("$$MessageSchema") && eventData.Properties["$$MessageSchema"].ToString().ToLower() == "truck-sensors;v1")
                {
                    Console.WriteLine("Parsing Truck Document...");

                    var newDocument = new
                    {
                        id = eventData.SystemProperties["iothub-connection-device-id"].ToString(),
                        processedUtcTime = DateTime.UtcNow,
                        enqueuedUtcTime = eventData.SystemProperties.EnqueuedTimeUtc,
                        deviceid = eventData.SystemProperties["iothub-connection-device-id"].ToString(),
                        devicetype = "truck",
                        location = new Point(parsed.longitude, parsed.latitude),
                        speed = parsed.speed,
                        speed_unit = parsed.speed_unit,
                        temperature = parsed.temperature,
                        temperature_unit = parsed.temperature_unit
                    };

                    Console.WriteLine("Truck document parsed: {0}", JsonConvert.SerializeObject(newDocument));

                    //Create the blob name and path for the new blob
                    DateTime timeNow = DateTime.Now;
                    string blobName = newDocument.deviceid + $"/" + timeNow.Year + $"/" + timeNow.Month + $"/" + timeNow.Day + $"/" + timeNow.Hour + $"/" + timeNow.ToLongTimeString() + ".json";

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                    // Set the blob's content type so that the browser knows to treat it as an image.
                    blockBlob.Properties.ContentType = "json";

                    var blobContent = JsonConvert.SerializeObject(newDocument);
                    await blockBlob.UploadTextAsync(blobContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Validates the connection string information in app.config and throws an exception if it looks like 
        /// the user hasn't updated this to valid values. 
        /// </summary>
        /// <returns>CloudStorageAccount object</returns>
        public CloudStorageAccount CreateStorageAccountFromConnectionString()
        {
            CloudStorageAccount storageAccount = null;
            const string Message = "Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.";

            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine(Message);
                Console.ReadLine();
            }
            catch (ArgumentException)
            {
                Console.WriteLine(Message);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return storageAccount;
        }
    }
}