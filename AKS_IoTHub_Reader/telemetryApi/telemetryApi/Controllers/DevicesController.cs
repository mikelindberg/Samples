using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using telemetryApi.Services;

namespace telemetryApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceEventsRepository repository;
        private readonly ILogger logger;

        public DevicesController(IDeviceEventsRepository repository, ILoggerFactory loggerFactory)
        {
            this.repository = repository;
            this.logger = loggerFactory.CreateLogger<DevicesController>();
        }

        // GET api/devices
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var deviceResults = await repository.GetDeviceEventsAsync();

            logger.LogInformation($"DevicesController - api/devices - getting from Cosmos");

            return Ok(deviceResults);
        }

        // GET api/devices/[deviceid]
        [HttpGet("{deviceid}")]
        public ActionResult<string> Get(string deviceid)
        {
            return "value";
        }

        
    }
}
