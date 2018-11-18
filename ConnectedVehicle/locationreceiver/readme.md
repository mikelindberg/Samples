# Location Receiver #
The Location Receiver is used to pull location data {deviceid, longitude, latitude, timestamp} from an event hub.

Connection strings for Event Hub and Blob storage are stored as secrets.

You need to add an appsettings.json file to the locationreceiver project. It should have the following structure:
```json
{
    "EventHubName": "{Event hub name}",
    "StorageContainerName": "{Storage container name}"
}
```
