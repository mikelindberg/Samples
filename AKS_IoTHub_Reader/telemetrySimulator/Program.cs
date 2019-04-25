using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using telemetryModel;

namespace telemetrySimulator
{
    class Program
    {
        static int Interval { get; set; } = 10000;
        static double centerLatitude = 47.25433;
        static double centerLongitude = -121.177075;

        static string iotDeviceConnectionString;
        static DeviceClient deviceClient;

        static Random rand = new Random();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {

                Console.WriteLine("Argument index 0 not found: the Device specific IoT Hub connection string. Press any key to exit.");
                Console.ReadLine();
                return;
            }
            else
            {
                iotDeviceConnectionString = args[0].ToString();
            }


            Console.WriteLine("Simulated device starting\n");

            InitDeviceClient().Wait();

            SendDeviceToCloudMessagesAsync();


            Console.ReadLine();

        }

        static public async Task InitDeviceClient()
        {

            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            // DEV only! bypass certs validation
            mqttSetting.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            ITransportSettings[] settings = { mqttSetting };

            deviceClient = DeviceClient.CreateFromConnectionString(iotDeviceConnectionString, settings);
            await deviceClient.OpenAsync();

            Console.WriteLine($"Connected to IoT Hub with connection string [{iotDeviceConnectionString}]");

        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;

            TruckDeviceEvent truck = new TruckDeviceEvent
            {
                temperature = minTemperature,
                longitude = centerLongitude,
                latitude = centerLatitude
            };

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                var (latitude, longitude) = GenerateNewCoordinates(truck.latitude, truck.longitude, 0.01);
                truck.longitude = longitude;
                truck.latitude = latitude;
                truck.speed = vary(30, 5, 0, 80);
                truck.temperature = currentTemperature;

                var messageString = JsonConvert.SerializeObject(truck);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                message.Properties.Add("$$MessageSchema", "truck-sensors;v1");

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(Interval);
            }
        }

        public static (double Latitude, double Longitude) GenerateNewCoordinates(double latitude, double longitude, double incrementKm)
        {
            //See sample https://github.com/Azure/device-simulation-dotnet/blob/master/Services/data/devicemodels/scripts/truck-02-state.js
            var radians = ((incrementKm * 1000) / 6378137) * (180 / Math.PI);
            var newLatitude = latitude + radians;
            var newLongitude = longitude + radians / Math.Cos(latitude * Math.PI / 180);

            return (newLatitude, newLongitude);
        }

        public static double vary(int avg, int percentage, int min, int max)
        {
            //See sample https://github.com/Azure/device-simulation-dotnet/blob/master/Services/data/devicemodels/scripts/truck-02-state.js
            var value = avg * (1 + ((percentage / 100) * (2 * rand.NextDouble() - 1)));
            value = Math.Max(value, min);
            value = Math.Min(value, max);
            return value;
        }
    }

}
