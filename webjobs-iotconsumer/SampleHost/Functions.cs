// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SampleHost.Filters;

namespace SampleHost
{
    [ErrorHandler]
    public class Functions
    {

        public void ProcessEvents([EventHubTrigger("sita-apc-flex", Connection = "TestEventHubConnection")] EventData[] events, ILogger log, Microsoft.Azure.EventHubs.Processor.PartitionContext partitionContext)
        {
            
            foreach (var evt in events)
            {
                
                log.LogTrace(new EventId(505, "Event Processed"),
                    $"Event processed (Offset={evt.SystemProperties.Offset}, " +
                    $"SequenceNumber={evt.SystemProperties.SequenceNumber}, " +
                    $"EnqueueTimeUtc={evt.SystemProperties.EnqueuedTimeUtc}, " +
                    $"EnqueueTime-Now={System.DateTime.UtcNow.Subtract(evt.SystemProperties.EnqueuedTimeUtc).TotalMilliseconds}, " +
                    $"PartitionId={partitionContext.PartitionId})");

            }
        }
    }
}
