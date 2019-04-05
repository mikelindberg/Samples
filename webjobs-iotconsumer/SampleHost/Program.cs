// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;

namespace SampleHost
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices()
                    .AddEventHubs();
                })
                .ConfigureAppConfiguration(b =>
                {
                    b.AddJsonFile("appsettings.json");
                })
                .ConfigureLogging((context, b) =>
                {
                    b.SetMinimumLevel(LogLevel.Information);

                    b.AddAzureWebAppDiagnostics();
                    //b.AddConsole();

                    // If this key exists in any config, use it to enable App Insights
                    //string appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                    //if (!string.IsNullOrEmpty(appInsightsKey))
                    //{
                    //    b.AddApplicationInsights(o => o.InstrumentationKey = appInsightsKey);
                    //}
                })
                .ConfigureServices(serviceCollection => serviceCollection
                .Configure<AzureFileLoggerOptions>(options =>
                {
                    options.FileName = "azure-diagnostics-";
                    options.FileSizeLimit = 50 * 1024;
                    options.RetainedFileCountLimit = 5;
                }).Configure<AzureBlobLoggerOptions>(options =>
                {
                    options.BlobName = "log.txt";
                }))
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
