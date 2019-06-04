using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace ClientSimulator
{
    class Program
    {
        static DeviceClient deviceClient;
        static int interval = 5000;
        static bool restart = false;

        static void Main(string[] args)
        {
            dynamic settings = ReadSettings();
            Task<bool> connectionEstablished = Task.Run(() => false);

            if (settings != null)
                connectionEstablished = CreateIoTHubClient(settings);

            if (!connectionEstablished.Result)
            {
                ProvisionDevice();
                settings = ReadSettings();
                connectionEstablished = CreateIoTHubClient(settings);
            }

            if (connectionEstablished.Result)
                Run().Wait();

            Console.ReadLine();
        }

        //Run a simulation loop
        static async Task Run()
        {
            Console.WriteLine("Starting to send telemetry!");
            Random rand = new Random();
            while (!restart)
            {
                var response = new SensorValue[]
                {
                    //Humidity percentage
                    new SensorValue() { TagName = "humidity_5", Value = 50 + rand.NextDouble() * 10, Type = SensorType.Humidity },
                    //Temperature in celcius
                    new SensorValue() { TagName = "temperature_1", Value = 23 + rand.NextDouble() * 5, Type = SensorType.Temperature }
                };

                var messageString = JsonConvert.SerializeObject(response);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.CreationTimeUtc = DateTime.UtcNow;

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                Thread.Sleep(interval);
            }
        }

        //Create a connection to the IoT Hub
        static async Task<bool> CreateIoTHubClient(dynamic settings, bool usePrimaryConnectionString = true)
        {
            if (settings == null)
                throw new Exception("Can't start IoT Hub connection based on an empty settings file!");

            Task<bool> success = Task.Run(() => true);

            Console.WriteLine("Starting IoT Hub connection");
            try
            {
                string iotDeviceConnectionString = usePrimaryConnectionString ? settings.primaryConnectionString : settings.secondaryConnectionString;
                MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
                ITransportSettings[] transportSettings = { mqttSetting };

                deviceClient = DeviceClient.CreateFromConnectionString(iotDeviceConnectionString, transportSettings);
                await deviceClient.OpenAsync();

                Console.WriteLine($"Connected to IoT Hub with connection string [{iotDeviceConnectionString}]");

                //read twin  setting upon first load
                var twin = await deviceClient.GetTwinAsync();
                await onDesiredPropertiesUpdate(twin.Properties.Desired, deviceClient);

                //register for Twin desiredProperties changes
                await deviceClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertiesUpdate, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (usePrimaryConnectionString)
                    success = CreateIoTHubClient(settings, false);
                else
                    success = Task.Run(() => false); ;
            }

            if (!success.Result)
                Console.WriteLine("Could not establish connection to IoT Hub");

            return success.Result;
        }

        static Task onDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties.Count > 0)
                {
                    if (desiredProperties["interval"] != null)
                        interval = desiredProperties["interval"];
                }

            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error when receiving desired property: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
            }
            return Task.CompletedTask;
        }

        //Provision the Device
        static void ProvisionDevice()
        {
            Console.WriteLine("Provisioning process starting..");
            Console.WriteLine("Fetching secrets..");
            var secrets = ReadSecrets();

            if (secrets == null)
            {
                Console.WriteLine("No secrets found!");
                return;
            }

            Console.WriteLine("Secrets found!");

            string dpsServiceEndpoint = secrets.serviceEndpoint;
            string idScope = secrets.idScope;
            string registrationId = secrets.registrationId;
            string primaryDeviceKey = secrets.primaryDerivedKey;
            string secondaryDeviceKey = secrets.secondaryDerivedKey;

            using (var security = new SecurityProviderSymmetricKey(registrationId, primaryDeviceKey, secondaryDeviceKey))
            {
                // Select one of the available transports:
                // To optimize for size, reference only the protocols used by your application.
                using (var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly))
                // using (var transport = new ProvisioningTransportHandlerHttp())
                // using (var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.TcpOnly))
                // using (var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.WebSocketOnly))
                {
                    try
                    {
                        Console.WriteLine("Creating provisioning client");

                        ProvisioningDeviceClient provClient =
                            ProvisioningDeviceClient.Create(dpsServiceEndpoint, idScope, security, transport);

                        Console.WriteLine("Starting attestation..");
                        DeviceRegistrationResult registrationResult = provClient.RegisterAsync().GetAwaiter().GetResult();
                        Console.WriteLine("Device successfully provisioned");

                        Console.WriteLine("Storing primary and secondary connection strings in settings file!");
                        Console.WriteLine("Primary Connection string: Hostname={0};DeviceId={1};SharedAccessKey={2}", registrationResult.AssignedHub, registrationId, primaryDeviceKey);
                        Console.WriteLine("Secondary Connection string: Hostname={0};DeviceId={1};SharedAccessKey={2}", registrationResult.AssignedHub, registrationId, secondaryDeviceKey);
                        string primaryConnectionString = "Hostname=" + registrationResult.AssignedHub + ";DeviceId=" + registrationResult.DeviceId + ";SharedAccessKey=" + primaryDeviceKey;
                        string secondaryConnectionString = "Hostname=" + registrationResult.AssignedHub + ";DeviceId=" + registrationResult.DeviceId + ";SharedAccessKey=" + secondaryDeviceKey;

                        WriteSettings(primaryConnectionString, secondaryConnectionString);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + ": " + ex.InnerException.Message);
                    }
                }
            }
        }

        //This should be replaced with your own logic for reading the secrets (registration id, primary key, secondary key)
        static dynamic ReadSecrets()
        {
            dynamic secrets = new
            {
                registrationId = "",
                primaryDeviceKey = "",
                secondaryDeviceKey = ""
            };

            try
            {
                secrets = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@".\secrets.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                secrets = null;
            }

            return secrets;
        }


        //Read the connection settings... This will be empty if the device has never connected
        static dynamic ReadSettings()
        {
            dynamic settings = null;

            try
            {
                settings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@".\settings.json"));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("No settings file exist!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return settings;
        }

        //Write the primary and secondary connection strings to a file after they have been created in the device provisioning process.
        static dynamic WriteSettings(string primaryConnectionString, string secondaryConnectionString)
        {
            dynamic settings = new
            {
                primaryConnectionString,
                secondaryConnectionString
            };

            try
            {
                File.WriteAllText(@".\settings.json", JsonConvert.SerializeObject(settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                settings = null;
            }

            return settings;
        }
    }

    public class SensorValue
    {
        public string TagName { get; set; }
        public double Value { get; set; }

        // Returns the text of the enum instead of number
        [JsonConverter(typeof(StringEnumConverter))]
        public SensorType Type { get; set; }
    }

    public enum SensorType
    {
        Humidity,
        Temperature
    }
}