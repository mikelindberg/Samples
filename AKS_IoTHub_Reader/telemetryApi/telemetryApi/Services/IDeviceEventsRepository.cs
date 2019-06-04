using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Spatial;
using telemetryApi.Models;

namespace telemetryApi.Services
{
    public interface IDeviceEventsRepository
    {
        Task<IEnumerable<TruckDeviceEventModel>> GetDeviceEventsAsync();

        Task<IEnumerable<TruckDeviceEventModel>> GetDeviceEventsAsync(Geometry zoomedArea);

    }
}
