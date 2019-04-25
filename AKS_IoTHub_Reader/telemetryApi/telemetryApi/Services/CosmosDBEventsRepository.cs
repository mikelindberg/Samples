using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using telemetryApi.Config;
using telemetryApi.Models;

namespace telemetryApi.Services
{
    public class CosmosDBEventsRepository : IDeviceEventsRepository
    {
        private string DatabaseName;
        private string CollectionName;
        private static DocumentClient client;
        private readonly ILogger logger;

        public CosmosDBEventsRepository(IOptions<CosmosDBConfig> cosmosConfig, ILoggerFactory loggerFactory)
        {
            var configOptions = cosmosConfig.Value;
            DatabaseName = configOptions.DatabaseName;
            CollectionName = configOptions.CollectionName;

            client = new DocumentClient(new Uri(configOptions.EndpointUri), configOptions.Key,
                new ConnectionPolicy {
                    RequestTimeout = new TimeSpan(0,30,0),
                    RetryOptions = new RetryOptions
                    {
                        MaxRetryAttemptsOnThrottledRequests = 10,
                        MaxRetryWaitTimeInSeconds = 30
                    }
                });

            logger = loggerFactory.CreateLogger<CosmosDBEventsRepository>();


        }

        public async Task<IEnumerable<TruckDeviceEventModel>> GetDeviceEventsAsync()
        {
            FeedOptions feedOptions = new FeedOptions {
                MaxItemCount = -1,
                EnableCrossPartitionQuery = true
            };

            IDocumentQuery<TruckDeviceEventModel> query = client.CreateDocumentQuery<TruckDeviceEventModel>(
                UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName), feedOptions)
                .AsDocumentQuery();

            var results = new List<TruckDeviceEventModel>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<TruckDeviceEventModel>());
            }

            logger.LogInformation($"CosmosDBEventsRepository - GetDeviceEventsAsync() returning list of {results.Count.ToString()} items");

            return results;
        }

        public async Task<IEnumerable<TruckDeviceEventModel>> GetDeviceEventsAsync(Geometry zoomedArea)
        {
            throw new NotImplementedException();
        }
    }
}
