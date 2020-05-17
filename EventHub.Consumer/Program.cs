using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventHub.Consumer
{
    class Program
    {
        private const string ehubNamespaceConnectionString = "from iothub";
        private const string eventHubName = "from iothub";
        private const string blobStorageConnectionString = "";
        private const string blobContainerName = "events";
        private static Container _container;

        static async Task Main(string[] args)
        {

            string databaseId = "EdgeElevator";
            string containerId = "State";
            string cosmosConnectionString = "";
            CosmosClient client = new CosmosClient(cosmosConnectionString, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Direct });
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseId);
            ContainerResponse container = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/id") { PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2 });
            _container = container.Container;

            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            BlobContainerClient storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);
            EventProcessorClient processor = new EventProcessorClient(storageClient, consumerGroup, ehubNamespaceConnectionString, eventHubName);

            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            await processor.StartProcessingAsync();
            await Task.Delay(TimeSpan.FromSeconds(60));
            await processor.StopProcessingAsync();
            Console.WriteLine("Hello World!");
        }

        static async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            string json = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
            Console.WriteLine("\tRecevied event: {0}", json);

            DeviceTelemetry telemetry = JsonConvert.DeserializeObject<DeviceTelemetry>(json);
            telemetry.Device = eventArgs.Data.SystemProperties["iothub-connection-device-id"].ToString();
            telemetry.Id = Guid.NewGuid().ToString();
            var response = await _container.CreateItemAsync(telemetry, new PartitionKey(telemetry.Id));
            Console.WriteLine(response.StatusCode);

            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        static Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }

    public class DeviceTelemetry
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "device")]
        public string Device { get; set; }

        [JsonProperty(PropertyName = "floor")]
        public string Floor { get; set; }
    }
}
