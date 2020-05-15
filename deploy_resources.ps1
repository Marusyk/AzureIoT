$location='westeurope'
$rg_name='iot-rg'
$hubname='iot-hub-research'
$hub_sku='F1'
$dps_name='iot-hub-dps-research'
$vault_name='iot-pki'
$cosmosDBAccountName='cosmos-db-research'

echo "Create resource group: $rg_name..."
az group create -n $rg_name -l $location

echo "Create Key Vault: $vault_name..."
az keyvault create --name $vault_name --resource-group $rg_name --location $location

echo "Create IoT Hub: $hubname..."
az iot hub create --name $hubname --resource-group $rg_name --sku $hub_sku --partition-count 2

echo "Create DPS: $hubname..."
az iot dps create -n $dps_name -g $rg_name 

echo "Get iot hub connection string..."
$hubConnectionString=$(az iot hub show-connection-string -n $hubname --key primary --query connectionString -o tsv)

echo "Link IoT Hub to DPS..."
az iot dps linked-hub create --dps-name $dps_name `
     -g $rg_name --connection-string $hubConnectionString `
     -l $location

echo "Create Cosmos DB: $cosmosDBAccountName..."
az cosmosdb create -n $cosmosDBAccountName -g $rg_name

az iot hub show-connection-string -n $hubname --policy-name service -o table
az cosmosdb list-keys --name $cosmosDBAccountName --resource-group $rg_name