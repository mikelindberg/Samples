using System;

using Microsoft.Azure.EventHubs;

using Microsoft.Azure.EventHubs.Processor;

using System.Threading.Tasks;

using System.Collections.Generic;

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.DataContracts;

namespace TestHostEPH
{

    public class SimpleEventProcessor : IEventProcessor
    {
 
        Microsoft.ApplicationInsights.TelemetryClient telemetry = null;

        public SimpleEventProcessor()
        {
            var config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration("e53c5bd5-6da4-4fa9-81eb-2a33cc6e1d0c");
            telemetry = new Microsoft.ApplicationInsights.TelemetryClient(config);
            telemetry.TrackTrace("SimpleEventProcessor initialized...", SeverityLevel.Information);
        }


        public Task CloseAsync(PartitionContext context, CloseReason reason)

        {

            telemetry.TrackTrace($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.", SeverityLevel.Information);

            return Task.CompletedTask;

        }



        public Task OpenAsync(PartitionContext context)

        {

            telemetry.TrackTrace($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'", SeverityLevel.Information);

            return Task.CompletedTask;

        }



        public Task ProcessErrorAsync(PartitionContext context, Exception error)

        {

            telemetry.TrackTrace($"Error on Partition: {context.PartitionId}, Error: {error.Message}", SeverityLevel.Information);

            return Task.CompletedTask;

        }



        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {

                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                telemetry.TrackTrace(
                    $"Event processed (Offset={eventData.SystemProperties.Offset}, " +
                    $"SequenceNumber={eventData.SystemProperties.SequenceNumber}), " +
                    $"EnqueueTimeUtc={eventData.SystemProperties.EnqueuedTimeUtc}, " +
                    $"EnqueueTime-Now={System.DateTime.UtcNow.Subtract(eventData.SystemProperties.EnqueuedTimeUtc).TotalMilliseconds}, " +
                    $"PartitionId={context.PartitionId}", SeverityLevel.Verbose);

            }
            return context.CheckpointAsync();

        }

    }

}