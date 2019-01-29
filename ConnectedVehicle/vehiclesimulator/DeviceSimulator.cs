using System;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace vehiclesimulator
{
    public class DeviceSimulator
    {
        private string deviceId = "";
        private string connectionString = "";
        static DeviceClient deviceClient;
        private static TransportType transportType = TransportType.Amqp;
        private Random random = new Random();
        private double litersInTank = 15.0;

        public DeviceSimulator()
        {

        }

        public DeviceSimulator(string connectionString)
        {
            this.connectionString = connectionString;
            deviceId = Regex.Match(connectionString, @"\b[DeviceId=]\w+?(?=;)\b").Value.Replace("=", "");
            Console.WriteLine(deviceId);
            var result = InitDevice();
        }

        public Task InitDevice()
        {
            try
            {
                deviceClient = DeviceClient.CreateFromConnectionString(connectionString, transportType);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            SendTelemetry();
            SendLocation();
            return Task.CompletedTask;
        }

        public async void SendTelemetry()
        {
            bool run = true;
            while (run)
            {
                try
                {
                    litersInTank = litersInTank < 0.5 ? 15.0 : litersInTank - 0.1;
                    var vehicleData = new VehicleData()
                    {
                        LitersInTank = litersInTank,
                        AvgSpeed = random.NextDouble() + 15 * 2,
                        Timestamp = DateTime.Now.ToString()
                    };

                    var jsonPayload = JsonConvert.SerializeObject(vehicleData);
                    var message = new Message(Encoding.UTF8.GetBytes(jsonPayload));
                    await deviceClient.SendEventAsync(message);

                    Console.WriteLine("Send: " + jsonPayload);
                }
                catch (System.Exception ex)
                {
                    try
                    {
                        await deviceClient.CloseAsync();
                        deviceClient.Dispose();
                    }
                    catch (System.Exception deviceException)
                    {
                        Console.WriteLine(deviceException.Message);
                    }
                    await InitDevice();
                    run = false;
                    Console.WriteLine(ex.Message);
                }
                await Task.Delay(10000);
            }
        }

        public async void SendLocation()
        {
            //If no device is set then don't send location data
            if (String.IsNullOrEmpty(deviceId))
                return;

            bool run = true;
            while (run)
            {
                try
                {
                    var locationData = new LocationData()
                    {
                        DeviceId = deviceId
                    };

                    var jsonPayload = JsonConvert.SerializeObject(locationData);
                    var message = new Message(Encoding.UTF8.GetBytes(jsonPayload));
                    await deviceClient.SendEventAsync(message);

                    Console.WriteLine("Send: " + jsonPayload);
                }
                catch (System.Exception ex)
                {
                    try
                    {
                        await deviceClient.CloseAsync();
                        deviceClient.Dispose();
                    }
                    catch (System.Exception deviceException)
                    {
                        Console.WriteLine(deviceException.Message);
                    }
                    await InitDevice();
                    run = false;
                    Console.WriteLine(ex.Message);
                }
                await Task.Delay(2000);
            }
        }
    }
}