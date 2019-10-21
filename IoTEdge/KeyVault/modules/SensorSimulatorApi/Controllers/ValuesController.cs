using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using util;

namespace SensorSimulatorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<SensorValue>> Get()
        {
            var simulatedValues = CreateSimulatedValues();
            return simulatedValues;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public ActionResult<IEnumerable<SensorValue>> Post([FromBody] string[] SensorIds)
        {
            var simulatedValues = CreateSimulatedValues();

            var valuesToReturn = 
            simulatedValues.Where(obj => SensorIds.Contains(obj.SensorId))
            .Select(obj => new SensorValue()
            {
                SensorId = obj.SensorId,
                Value = obj.Value,
                Type = obj.Type
            }).ToArray();

            return valuesToReturn;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private SensorValue[] CreateSimulatedValues()
        {
            var rand = new Random();

            var response = new SensorValue[]
            {
                //Co2 350 - 1000 per million parts (ppm)
                new SensorValue(){SensorId = "co2_1", Value = rand.NextDouble() * 1000, Type = SensorType.Co2},
                new SensorValue(){SensorId = "co2_2", Value = rand.NextDouble() * 1000, Type = SensorType.Co2},
                new SensorValue(){SensorId = "co2_3", Value = rand.NextDouble() * 1000, Type = SensorType.Co2},
                new SensorValue(){SensorId = "co2_4", Value = rand.NextDouble() * 1000, Type = SensorType.Co2},
                new SensorValue(){SensorId = "co2_5", Value = rand.NextDouble() * 1000, Type = SensorType.Co2},

                //Humidity percentage
                new SensorValue(){SensorId = "humidity_1", Value = 50 + rand.NextDouble() * 10, Type = SensorType.Humidity},
                new SensorValue(){SensorId = "humidity_2", Value = 50 - rand.NextDouble() * 10, Type = SensorType.Humidity},
                new SensorValue(){SensorId = "humidity_3", Value = 50 + rand.NextDouble() * 10, Type = SensorType.Humidity},
                new SensorValue(){SensorId = "humidity_4", Value = 50 - rand.NextDouble() * 10, Type = SensorType.Humidity},
                new SensorValue(){SensorId = "humidity_5", Value = 50 + rand.NextDouble() * 10, Type = SensorType.Humidity},

                //Temperature in celcius
                new SensorValue(){SensorId = "temperature_1", Value = 23 + rand.NextDouble() * 5, Type = SensorType.Temperature},
                new SensorValue(){SensorId = "temperature_2", Value = 23 - rand.NextDouble() * 5, Type = SensorType.Temperature},
                new SensorValue(){SensorId = "temperature_3", Value = 23 + rand.NextDouble() * 5, Type = SensorType.Temperature},
                new SensorValue(){SensorId = "temperature_4", Value = 23 - rand.NextDouble() * 5, Type = SensorType.Temperature},
                new SensorValue(){SensorId = "temperature_5", Value = 23 + rand.NextDouble() * 5, Type = SensorType.Temperature},

                //PIR
                new SensorValue(){SensorId = "PIR_1", Value = /*rand.NextDouble() > 0.5 ? 1 :*/ 0, Type = SensorType.PIR},
                new SensorValue(){SensorId = "PIR_2", Value = rand.NextDouble() > 0.5 ? 1 : 0, Type = SensorType.PIR},
                new SensorValue(){SensorId = "PIR_3", Value = rand.NextDouble() > 0.5 ? 1 : 0, Type = SensorType.PIR},
                new SensorValue(){SensorId = "PIR_4", Value = rand.NextDouble() > 0.5 ? 1 : 0, Type = SensorType.PIR},
                new SensorValue(){SensorId = "PIR_5", Value = rand.NextDouble() > 0.5 ? 1 : 0, Type = SensorType.PIR},
            };

            foreach (var sensorValue in response.Where(obj => obj.Type == SensorType.Co2))
                sensorValue.Value = sensorValue.Value < 350 ? sensorValue.Value + 350 : sensorValue.Value;

            return response;
        }
    }
}
