# DPS with Symmetric Keys

In this sample you will create an IoT Hub to hold your devices, an IoT device provisioning service for zero-touch provisioning, a console app for creating Symmetric keys and an IoT simulator.

## What is Azure IoT Hub Device Provisioning Service?
The IoT Hub Device Provisioning Service is a helper service for IoT Hub that enables zero-touch, just-in-time provisioning to the right IoT hub without requiring human intervention, enabling customers to provision millions of devices in a secure and scalable manner.

To learn more about Azure IoT Hub DPS, go to the [documentation site](https://docs.microsoft.com/en-us/azure/iot-dps/about-iot-dps)

## Prerequisites
- An Azure Subscription
- Azure CLI installed [Link](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
- Clone this sample code

## The sample code
This sample consist of 2 projects.
- CreateDerivedKey: This project is used to create a unique key for a device. In real world scenarios this will often be done at the factory level before devices are shipped to end users.
- ClientSimulator: This project is used to simulate a device. When the device starts it will use the unique key created in the CreateDerivedKey project.

## Try it!

### Create a resource group

You need a resource group to have a place to store the Azure components you will use in the sample (Azure IoT Hub and Azure DPS).

To create a resource group execute the following script which will add a new resource group in your Azure subscription with the name dpstest and the location in westeurope.

```
az group create -n dpstest -l westeurope
```

You should be able to see the resource group in your Azure subscription.

### Create a Azure IoT Hub

You can use Azure IoT Hub to store our devices once they have been provisioned. If you have not worked with Azure IoT Hub before then you can learn more [here](https://docs.microsoft.com/en-us/azure/iot-hub/about-iot-hub). 

To create an Azure IoT Hub in your new resource group run the following script with your own unique name (remove the <>).

```
az iot hub create -n <give your hub a unique name> -g dpstest -l westeurope
```

It can take a few minutes to execute the script above.

After completion you should be able to see your IoT Hub in your resource group.

### Create a Azure IoT Device Provisioning Service

 The IoT Hub Device Provisioning Service is a helper service for IoT Hub that enables zero-touch, just-in-time provisioning to the right IoT hub without requiring human intervention, enabling customers to provision millions of devices in a secure and scalable manner.

#### Create the service
 To setup the Device Provisioning Service run the following script.

 ```
az iot dps create -n mytestdps -g dpstest -l westeurope
 ```
##### Copy the idScope and serviceOperationsHostName... you will need them later!

#### Link DPS to your IoT Hub
 After the DPS is created then you need to link it to your IoT Hub. To do that run the following script. Remember to set your own IoT Hub connection string. The connection string can be found under Shared access policies / iothubowner (remove the <> but not " ").

 ```
 az iot dps linked-hub create --connection-string "<the IoT Hub connection string>" --dps-name mytestdps -l westeurope -g dpstest
 ```

#### Create an enrollment group
After there is a link between the IoT Hub and the provisioning service, then you need to create an enrollment group.

The process of making the Device Provisioning Service instance aware of the devices that will attempt to register in the future. Enrollment is accomplished by configuring device identity information in the provisioning service, as either an "individual enrollment" for a single device, or a "group enrollment" for multiple devices. Identity is based on the attestation mechanism the device is designed to use, which allows the provisioning service to attest to the device's authenticity during registration.

To setup the enrollment group, execute the following script. 
 ```
 az iot dps enrollment-group create --dps-name mytestdps --enrollment-id testgroup -g dpstest --allocation-policy hashed
 ```

Copy the primary and secondary symmetric keys, so you can use them to create a derived key in the next step.

### Create a symmetric key from the master key
Before you continue then you should read the Group enrollment documentation [here](https://docs.microsoft.com/en-us/azure/iot-dps/concepts-symmetric-key-attestation#group-enrollments)

In the documentation above there is a code example of creating a derived key. This sample uses the same code to create that key.

 ```csharp
 public static string ComputeDerivedSymmetricKey(byte[] masterKey, string registrationId)
        {
            using (var hmac = new HMACSHA256(masterKey))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(registrationId)));
            }
        }
 ```

Move to the root folder of this sample so that you are in the DPS_SymmetricKeys folder.

Open cmd, PowerShell or Terminal and execute the following script which will restore the CreateDerivedKey and ClientSimulator projects.

```
dotnet build
```

After the build is completed you can generate your derived key by using the primary and secondary keys. If you don't have the keys, then they can be found in the [Azure Portal](portal.azure.com) by following these steps:

1. On the left-hand side in the Azure portal select Resource groups.
2. In the list of Resource groups select the group you created earlier.
3. In your resource group select your Device Provisioning Service solution.
4. In the Device Provisioning Service menu you select Manage enrollments
5. Select the enrollment you created earlier.
6. Now you should see your primary and secondary keys. Copy them.

You will also need your DPS service endpoint and Id scope. If you did not copy those earlier then they can be found on the Overview page of the DPS.

With the keys copied you can run the following script. 

Make sure you are still in the DPS_SymmetricKeys directory!

##### Windows
```
dotnet run --project .\CreateDerivedKey\CreateDerivedKey.csproj "<DPS Endpoint>" "id scope" <Unique device name e.g. serialnumber and MAC address>" "<Your primary key>" "<Your secondary key>"
```

##### Linux
```
dotnet run --project ./CreateDerivedKey/CreateDerivedKey.csproj "<DPS Endpoint>" "id scope" <Unique device name e.g. serialnumber and MAC address>" "<Your primary key>" "<Your secondary key>"
```

The script above runs the CreateDerivedKey project which will create two new symmetric keys (primary and secondary) which derived from the master keys of your enrollment group. The new keys together with your unique registration/device id is stored in a file called secrets.json (In an actual scenario this should be stored in a secure location on the device.) 

To understand the code go through the Program.cs file in the CreateDerivedKey project and read about [HMACSHA256](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha256?view=netframework-4.8)

### Connect your device to the cloud for the first time

Now that you have your registration/device id, primary key and secondary key stored safely on your device (or as in our example just in a json file) you can connect to the DPS service.

When you device connects the following process steps are performed.

1. Device contacts the provisioning service endpoint set at the factory. The device passes the identifying information to the provisioning service to prove its identity.
2. The provisioning service validates the identity of the device by validating the registration ID and key against the enrollment list entry.
3. The provisioning service registers the device with an IoT hub and populates the device's desired twin state.
4. The IoT hub returns device ID information to the provisioning service.
5. The provisioning service returns the IoT hub connection information to the device. The device can now start sending data directly to the IoT hub.
6. The device connects to IoT hub.
7. The device gets the desired state from its device twin in IoT hub.

To connect your device simply run the following script:

##### Windows
```
dotnet run --project .\ClientSimulator\ClientSimulator.csproj 
```

##### Linux
```
dotnet run --project ./ClientSimulator/ClientSimulator.csproj 
```

Your device will now try and establish connection to the cloud by using the derived keys. After it get's the IoT Hub information needed to connect it will create a new file called settings.json where the primary and secondary connectionstrings are present.

After you get the initial connection you can try and stop the simulator, delete the settings file, regenerate your enrollment group Primary key in Azure, restart the simulator... What happens?