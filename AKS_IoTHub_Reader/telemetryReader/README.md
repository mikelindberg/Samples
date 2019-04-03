# Telemetry Reader

For the telemetry reader to work you need to add a settings file in the following json format.

```json
{
    "EventHubName": <"your event hub name">,
    "EventHubConnectionString": <"your event hub connection string">,
    "StorageContainerName": <"container name for pointer">,
    "StorageConnectionString": <"storage connection string">
}
```