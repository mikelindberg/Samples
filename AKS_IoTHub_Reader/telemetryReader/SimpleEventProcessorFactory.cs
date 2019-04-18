using System;
using Microsoft.Azure.EventHubs.Processor;

namespace telemetryReader
{
    public class SimpleEventProcessorFactory : IEventProcessorFactory
    {
        private readonly IDocumentService _documentService;

        public SimpleEventProcessorFactory(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            Console.WriteLine($"Creating event processor for partition {context.PartitionId}.");
            return new SimpleEventProcessor(_documentService);
        }
    }
}