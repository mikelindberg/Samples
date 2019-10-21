namespace Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureHelpers;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using util;

    public class Program
    {
        static int interval = 30000;
        static string apipath = "http://127.0.0.1:5000/";
        static ModuleClient ioTHubModuleClient = null;
        static SensorValue[] sensorList;
        static string blobName;
        static string Username = "";
        static string Password = "";
        // static ApiModels.Token Token;
        static KeyVaultHelper keyVaultHelper;

        public static void Main(string[] args)
        {
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
                //Load saved sensor list
                if (File.Exists(Environment.CurrentDirectory + @"/sensorlist.json"))
                {
                    using (var reader = new StreamReader(Environment.CurrentDirectory + @"/sensorlist.json"))
                    {
                        sensorList = JsonConvert.DeserializeObject<SensorValue[]>(reader.ReadToEnd());
                    }
                    Console.WriteLine(sensorList);
                }
                else
                {
                    sensorList = new SensorValue[0];
                }

                MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
                ITransportSettings[] settings = { mqttSetting };

                // Open a connection to the Edge runtime
                ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
                await ioTHubModuleClient.OpenAsync();
                Console.WriteLine("IoT Hub module client initialized.");

                //read twin  setting upon first load
                var twin = await ioTHubModuleClient.GetTwinAsync();
                await onDesiredPropertiesUpdate(twin.Properties.Desired, ioTHubModuleClient);

                //register for Twin desiredProperties changes
                await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(onDesiredPropertiesUpdate, null);
            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.WriteLine($"Error in Init: {exc.Message}");
            }
            Run();
        }

        // Handle property updates
        static async Task onDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                var blobToken = "";

                if (desiredProperties.Count > 0)
                {
                    if (desiredProperties.Contains("interval"))
                        interval = desiredProperties["interval"];
                    if (desiredProperties.Contains("apipath"))
                        apipath = desiredProperties["apipath"];
                    if (desiredProperties.Contains("blobSetting"))
                    {
                        // Console.WriteLine(desiredProperties["blobSetting"]);
                        // var blobSetting = JsonConvert.DeserializeObject<dynamic>(desiredProperties["blobSetting"]);
                        var blobSetting = new BlobSetting()
                        {
                            blobName = desiredProperties["blobSetting"].blobName + "",
                            blobToken = desiredProperties["blobSetting"].blobToken + ""
                        };

                        Console.WriteLine(Type.GetType(blobSetting.blobName));
                        Console.WriteLine(Type.GetType(blobSetting.blobToken));

                        var storeListTask = await StoreEdgeDataList(blobSetting.blobToken);

                        blobName = blobSetting.blobName;
                        blobToken = blobSetting.blobToken;
                    }
                    if (desiredProperties.Contains("KeyVault"))
                    {
                        Console.WriteLine(" KeyVault content: " + desiredProperties["KeyVault"]);

                        //Get the properties to create a new instance of the keyvault helper
                        var keyVaultObject = desiredProperties["KeyVault"];
                        //Loop the secretnames "array"
                        List<string> secretNames = new List<string>();
                        foreach (var config in keyVaultObject.secretNames)
                        {
                            secretNames.Add(config.First.secretName.ToString());
                        }

                        //Create new keyvault helper instance
                        keyVaultHelper = new KeyVaultHelper(keyVaultObject.keyVaultUrl.ToString(), keyVaultObject.clientId.ToString(), keyVaultObject.clientSecret.ToString(), secretNames.ToArray());

                        // //Get username and password from the Keyvault helper
                        Username = keyVaultHelper.GetSecret("ClientUserName").GetAwaiter().GetResult().Value.ToString();
                        Password = keyVaultHelper.GetSecret("ClientPassword").GetAwaiter().GetResult().Value.ToString();
                        Console.WriteLine("user: {0}, pass: {1}", Username, Password);
                        //Get a new token for the  system
                        // Token = await GetApiToken();
                    }
                }

                var propertiesToUpdate = new
                {
                    interval = interval,
                    apipath = apipath,
                    blobSetting = new { blobName = blobName, blobToken = blobToken }
                };

                var reportedProperties = new TwinCollection(JsonConvert.SerializeObject(propertiesToUpdate));
                await ioTHubModuleClient.UpdateReportedPropertiesAsync(reportedProperties);
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
        }

        static async Task<ApiModels.Token> GetApiToken()
        {
            ApiModels.Token token = null;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(apipath);
                    client.DefaultRequestHeaders.Accept.Clear();

                    var requestBody = "grant_type=password&username=" + Username + "&password=" + Password;
                    Console.WriteLine(requestBody);
                    var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
                    // Get sensor values
                    HttpResponseMessage response = await client.PostAsync(apipath + "api/token", httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var stringResponse = await response.Content.ReadAsStringAsync();
                        token = JsonConvert.DeserializeObject<ApiModels.Token>(stringResponse);
                        Console.WriteLine("New token retreived, expires in {0} seconds", token.expires_in);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR - GetSensorValues {e.Message}");
                    token = null;
                }
            }
            return token;
        }

        static async Task<bool> StoreEdgeDataList(string sasToken)
        {
            bool success = false;
            try
            {
                var newSensorList = await DownloadSensorList(sasToken);
                sensorList = JsonConvert.DeserializeObject<SensorValue[]>(newSensorList);
                Console.WriteLine("New sensor values: " + newSensorList);

                using (var writer = new StreamWriter(Environment.CurrentDirectory + @"/sensorlist.json"))
                {
                    await writer.WriteAsync(newSensorList);
                }
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - StoreEdgeDataList {ex.Message}");
            }
            return success;
        }

        //Use the blob SAS token to download the file
        static async Task<string> DownloadSensorList(string sasToken)
        {
            string content = "";
            try
            {
                //Create a new blockBlob instance by using the SAS token
                var blob = new CloudBlockBlob(new Uri(sasToken));

                Stream readStream = blob.OpenReadAsync().Result;
                using (var reader = new StreamReader(readStream))
                {
                    content = await reader.ReadToEndAsync();
                }
                readStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - DownloadSensorList {ex.Message}");
            }

            return content;
        }

        static async void Run()
        {
            while (true)
            {
                var sensorValues = await GetSensorValues();
                await UploadSensorValues(sensorValues);

                Thread.Sleep(interval);
            }
        }

        // Get all sensorvalues from the web service
        static async Task<SensorValue[]> GetSensorValues()
        {
            SensorValue[] sensorValues = new SensorValue[0];
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(apipath);
                    client.DefaultRequestHeaders.Accept.Clear();

                    var requestBody = JsonConvert.SerializeObject(sensorList.Select(obj => obj.SensorId).ToArray());
                    Console.WriteLine(requestBody);
                    var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    // Get sensor values
                    HttpResponseMessage response = await client.PostAsync(apipath + "api/values", httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var stringResponse = await response.Content.ReadAsStringAsync();
                        sensorValues = JsonConvert.DeserializeObject<SensorValue[]>(stringResponse);
                        Console.WriteLine("{0} sensor values retreived", sensorValues.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR - GetSensorValues {e.Message}");
                }
            }

            return sensorValues;
        }

        //Upload all sensorvalues to Azure IoT Hub
        static async Task UploadSensorValues(SensorValue[] sensorValues)
        {
            try
            {
                var messageString = JsonConvert.SerializeObject(sensorValues);
                Console.WriteLine("Sending message: " + messageString);

                if (!string.IsNullOrEmpty(messageString))
                {
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));
                    message.CreationTimeUtc = DateTime.UtcNow;

                    await ioTHubModuleClient.SendEventAsync("sensoroutput", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - UploadSensorValues - {ex.Message}");
            }

            return;
        }

        public class BlobSetting
        {
            public string blobName { get; set; }
            public string blobToken { get; set; }
        }
    }
}