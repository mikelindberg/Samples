namespace controller_interface_module
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Newtonsoft.Json;
    using Microsoft.Azure.Devices.Shared;

    class Program
    {
        static int counter = 0;
        static SerialInterface _serialInterface;
        static DeviceClient iotClient;
        static PropertyCollection propertyCollection;

        static void Main(string[] args)
        {
            _serialInterface = new SerialInterface();

            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            try
            {
                MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
                ITransportSettings[] settings = { mqttSetting };

                // Open a connection to the Edge runtime
                // ModuleClient iotClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
                iotClient = DeviceClient.CreateFromConnectionString("HostName=robtoarmhub.azure-devices.net;DeviceId=testDevice;SharedAccessKey=xpx0qcbBIUHBn+2ah14g6rgwSTyK/LbaZBPeiOIlj0s=", settings);
                await iotClient.OpenAsync();
                Console.WriteLine("IoT Hub module client initialized.");

                Twin moduleTwin = await iotClient.GetTwinAsync();
                propertyCollection = JsonConvert.DeserializeObject<PropertyCollection>(moduleTwin.Properties.Desired.ToJson());

                Console.WriteLine(propertyCollection.ToString());

                //callback for generic direct method calls
                iotClient.SetMethodHandlerAsync("movearm", MoveArm, iotClient).Wait();

                // Register callback to be called when a message is received by the module
                // await iotClient.SetInputMessageHandlerAsync("input1", PipeMessage, iotClient);

                //register for Twin desiredProperties changes
                await iotClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertiesUpdate, null);

                setAvailablePortsProperty();

                TrySerialConnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not initate IoT client");
                Console.WriteLine(ex.Message);
            }

            run();
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                var pipeMessage = new Message(messageBytes);
                foreach (var prop in message.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                }
                await moduleClient.SendEventAsync("output1", pipeMessage);
                Console.WriteLine("Received message sent");
            }
            return MessageResponse.Completed;
        }

        static Task onDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties.Count > 0)
                {
                    if (desiredProperties["portName"] != null)
                        propertyCollection.portName = desiredProperties["portName"];

                    if (desiredProperties["baudRate"] != null)
                        propertyCollection.baudRate = desiredProperties["baudRate"];
                }

                //Check if there is a connection to the serial port.
                TrySerialConnect();

                //Update the reported properties
                UpdateReportProperties();
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

        //Retreive the possible COM ports and update the reported properties
        static void setAvailablePortsProperty()
        {
            propertyCollection.availablePorts = _serialInterface.getAvailablePortsProperty();
            UpdateReportProperties();
        }

        static Task<MethodResponse> MoveArm(MethodRequest methodRequest, object userContext)
        {
            string result = "'DM call success'";

            try
            {
                MoveArm movearm = JsonConvert.DeserializeObject<MoveArm>(methodRequest.DataAsJson);
                var command = new Command(CommandType.mArm, movearm);

                _serialInterface.WriteSerial(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(new MethodResponse(System.Text.Encoding.UTF8.GetBytes(ex.Message), 999));
            }

            return Task.FromResult(new MethodResponse(System.Text.Encoding.UTF8.GetBytes(result), 200));
        }

        static void run()
        {
            while (true)
            {
                if (propertyCollection.isSerialConnected)
                {
                    string message = _serialInterface.ReadSerial();
                    Console.WriteLine(message);
                }

                Task.Delay(1000);
            }
        }

        static async void UpdateReportProperties()
        {
            var twinCollection = new TwinCollection(JsonConvert.SerializeObject(propertyCollection));
            await iotClient.UpdateReportedPropertiesAsync(twinCollection);
        }

        static async void TrySerialConnect()
        {
            if (propertyCollection.isSerialConnected)
            {
                propertyCollection.isSerialConnected = _serialInterface.CloseSerialConnection();
                await Task.Delay(2000);
                Console.WriteLine("Is serial connection open? " + propertyCollection.isSerialConnected);
            }

            _serialInterface.SetPortName(propertyCollection.portName);
            _serialInterface.SetPortBaudRate(propertyCollection.baudRate);

            propertyCollection.isSerialConnected = _serialInterface.OpenSerialConnection();
            Console.WriteLine("Is connected: " + propertyCollection.isSerialConnected);

            if (propertyCollection.isSerialConnected)
                _serialInterface.WriteSerial(new Command(CommandType.Test, "Test payload"));
        }
    }

    public class PropertyCollection
    {
        public string availablePorts { get; set; } = "";
        public string portName { get; set; } = "";
        public int baudRate { get; set; } = 0;
        public bool isSerialConnected { get; set; }

        public override string ToString()
        {
            StringBuilder toString = new StringBuilder();

            toString.AppendLine("Available Ports: " + availablePorts);
            toString.AppendLine("Selected Port Name: " + portName);
            toString.AppendLine("Selected baudrate: " + baudRate);
            toString.AppendLine("Is Serial Connected?: " + isSerialConnected);

            return toString.ToString();
        }
    }
}