# Description

This repository contains all required scripts and source code to deploy iot hub sample infrastructure

## Dependencies
Before moving to infrustructure deployment and run of sample application please make sure that following conditions were met:

1. Installed Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
2. Installed all necessary extensions for Azure CLI. To do this just run script below:
```PowerShell
az extension add --name azure-cli-iot-extps1
npm install -g azure-functions-core-tools
```
4. Login into Azure Account:
```PowerShell
az login
```
5. Create resources
```PowerShell
.\deploy_resources.ps1
```
6. Create a new Application in Azure Active Directory with name `RootCertificate.Setup`
7. Open `RootCertificate.Setup` app -> Authentication -> add Web platform and set with `https://localhost` as a Redirect URI. Default client type set to `Yes`
8. Add thie app to Access Policy of KeyVault service
9. Generate Root certificate on your machine and store it to KeyVault
```PowerShell
cd RootCertificate.Setup
dotnet run
```
10. Deoloy Azure Function. 
11. Got to Platform features of function -> Identity -> set Status to `On`
12. Open Authentication / Authorization of function and set App Service Authentication to `On`. Choose `Log in with Azure Active Directory` and configure Active Directory Authentication. Create a new app with name `EdgeDevice.RequestCertificate` and Save
13. Configure Function settings: connection string to blob and container name
14. Add `EdgeDevice.RequestCertificate` app to Access Policy of KeyVault service with Key Permissions = Sign, Secret Permissions = None, Certificate Permissions = Get
15. Go to DPS service
16. Add Certificate `RootCert.cer` from your machine or from Vault, name it `CA`. Copy code from portal.
17. Go to `appsetting.json` of `EdgeDevice.RequestCertificate` project. Set `ClientId` to Id of `EdgeDevice.RequestCertificate` app from Azure. Set `TenantId` of app and `BaseUrl` to URL of Azure function (https://certauthorityfunction.azurewebsites.net). Run

```PowerShell
cd EdgeDevice.RequestCertificate
dotnet run
```
18. Set verification code as a device name.
19. Upload generated `IssuedCertificate.cer` to portal and press `Verify`
20. Create enrollment group and specify CA certificate
21. Now you can create certificate for any devices. Specify `DeviceName` and run:
```PowerShell
cd EdgeDevice.RequestCertificate
dotnet run
```
22. Go to appsettings.json of `EdgeDevice.Simulator` project. Set `DeviceName` - from previous step. Set `DpsIdScope` from DPS service. Run simulator:
```PowerShell
cd EdgeDevice.Simulator
dotnet run
```
23. Run WebApi service to send command to device
```PowerShell
cd Evelator.Api
dotnet run
```
24. Send request
```sh
 curl -X POST -k https://localhost:5001/Elevator -H "Content-Type: application/json" -d "{ \"deviceName\": \"elevator1\", \"floor\": 17 }"
```

https://www.youtube.com/watch?v=7K2IuqJKAPc
