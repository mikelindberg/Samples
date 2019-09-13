using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace telemetryReader{
    public interface IDocumentService
    {
        Task StoreDocument(EventData eventData);
    }
}