using System;
using System.Collections.Generic;
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
using Sql = System.Data.SqlClient;

namespace SqlClient
{
    class Program
    {
        static ModuleClient ioTHubModuleClient;
        static int lastId = 0;
        static string str = "Data Source=tcp:172.18.0.3,1433;Initial Catalog=MeasurementsDB;User Id=SA;Password=Strong!Passw0rd;TrustServerCertificate=False;Connection Timeout=30;";

        static void Main(string[] args)
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
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");
            await Run();
        }

        async static Task Run()
        {
            while (true)
            {
                try
                {
                    var maxId = GetMaxId();

                    if (maxId > 0)
                    {
                        List<TemperatureData> upstreamData = GetLatestData(maxId);
                        await SendSqlData(upstreamData);

                        lastId = maxId;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(10000);
            }
        }

        public static int GetMaxId()
        {
            int maxId = 0;
            try
            {
                using (Sql.SqlConnection conn = new Sql.SqlConnection(str))
                {
                    conn.Open();
                    var maxIdQuery = "SELECT MAX(id) FROM MeasurementsDB.dbo.TemperatureMeasurements";
                    using (Sql.SqlCommand cmd = new Sql.SqlCommand(maxIdQuery, conn))
                    {
                        //Execute the command and log the # rows affected.
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            maxId = Convert.ToInt32(reader[0]);
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return maxId;
        }

        public static List<TemperatureData> GetLatestData(int maxId)
        {
            var latestData = new List<TemperatureData>();
            try
            {
                using (Sql.SqlConnection conn = new Sql.SqlConnection(str))
                {
                    conn.Open();
                    var newDataQuery = "SELECT * FROM MeasurementsDB.dbo.TemperatureMeasurements WHERE id > " + lastId + " AND id <= " + maxId;

                    using (Sql.SqlCommand cmd = new Sql.SqlCommand(newDataQuery, conn))
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var tempData = new TemperatureData()
                            {
                                id = Convert.ToInt32(reader["id"]),
                                measurementTime = Convert.ToDateTime(reader["measurementTime"]),
                                location = Convert.ToString(reader["location"]),
                                temperature = Convert.ToDouble(reader["temperature"])
                            };

                            latestData.Add(tempData);
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return latestData;
        }
        public static async Task<bool> SendSqlData(List<TemperatureData> temperatureData)
        {
            var success = false;

            try
            {
                var serializedData = JsonConvert.SerializeObject(temperatureData);
                byte[] messageBytes = Encoding.ASCII.GetBytes(serializedData);
                var message = new Message(messageBytes);

                await ioTHubModuleClient.SendEventAsync("output1", message);
                Console.WriteLine(DateTime.Now.ToString() + " -- Sent message: " + serializedData);

                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return success;
        }
    }
    class TemperatureData
    {
        public int id { get; set; }
        public DateTime measurementTime { get; set; }
        public string location { get; set; }
        public double temperature { get; set; }
    }
}
