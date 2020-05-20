using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventHub.Consumer
{
    internal static class EventHubConsumer
    {
        private const string ehubNamespaceConnectionString = "";
        private const string eventHubName = "";
        private const string blobStorageConnectionString = "";
        private const string blobContainerName = "events";

        internal static async Task Consume()
        {
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            BlobContainerClient storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);
            EventProcessorClient processor = new EventProcessorClient(storageClient, consumerGroup, ehubNamespaceConnectionString, eventHubName);

            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            await processor.StartProcessingAsync();
            await Task.Delay(TimeSpan.FromSeconds(60));
            await processor.StopProcessingAsync();
        }

        static async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            string json = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
            Console.WriteLine("\tRecevied event: {0}", json);

            DeviceTelemetry telemetry = JsonConvert.DeserializeObject<DeviceTelemetry>(json);
            telemetry.Device = eventArgs.Data.SystemProperties["iothub-connection-device-id"].ToString();
            telemetry.Id = Guid.NewGuid().ToString();
            //var response = await _container.CreateItemAsync(telemetry, new PartitionKey(telemetry.Id));
            //Console.WriteLine(response.StatusCode);

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
}
