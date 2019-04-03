# Telemetry Reader

For the telemetry reader to work you need to add a json file called appsettings.json with the following content.

```json
{
    "EventHubName": "<your event hub name>",
    "EventHubConnectionString": "<your event hub connection string>",
    "StorageContainerName": "<container name for pointer>",
    "StorageConnectionString": "<storage connection string>"
}
```