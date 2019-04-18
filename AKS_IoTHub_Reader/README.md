# AKS IoT Hub Reader

This sample is a walkthrough of using AKS (Azure Kubernetes Service) and ACI (Azure Container Instances) to host different applications used in an IoT scenario.

In the sample you will setup and IoT Hub for ingesting device data to Azure, read that data using an EventProcessorHost, storing the data in CosmosDB and visualizing the data in a React application.

## Pre-requisites
- Visual Studio Code
- .NET Core SDK 2.2 - 
- Kubernetes CLI - https://kubernetes.io/docs/tasks/tools/install-kubectl/
- Azure CLI - https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
- Yarn - https://yarnpkg.com/lang/en/docs/install/#mac-stable

## Create a resource group
A resource group

## Setting up Azure Kubernetes Service

Azure Kubernetes Service (AKS) makes it simple to deploy a managed Kubernetes cluster in Azure. AKS reduces the complexity and operational overhead of managing Kubernetes by offloading much of that responsibility to Azure. As a hosted Kubernetes service, Azure handles critical tasks like health monitoring and maintenance for you. The Kubernetes masters are managed by Azure. You only manage and maintain the agent nodes

If you have never worked with Kubernetes before then I would suggest to watch this [YouTube](https://www.youtube.com/watch?v=4ht22ReBjno) video.

## Setting up Azure Container Registry

## Setting up Azure blob container

## Setting up Azure IoT Hub

### What is Azure IoT Hub?
IoT Hub is a managed service, hosted in the cloud, that acts as a central message hub for bi-directional communication between your IoT application and the devices it manages. You can use Azure IoT Hub to build IoT solutions with reliable and secure communications between millions of IoT devices and a cloud-hosted solution backend. You can connect virtually any device to IoT Hub.

### Provision an Azure IoT Hub


## Setting up CosmosDB 

### Create CosmosDB account

### Create a new database and collection
Database name: trucks
Collection Id: telemetry
You can choose other names if you prefer.

## Deploying the nodes to AKS
 
### Execute the following command to create a Kubernetes secret (this is a current workaround because ACI virtual nodes do not yet support service principals)
```
kubectl create secret docker-registry acrcredts --docker-server=[youracrname].azurecr.io --docker-username=[youracrname] --docker-password=[youracrpwd] --docker-email=[avalidemail]
```
