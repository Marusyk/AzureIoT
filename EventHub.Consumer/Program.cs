using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace EventHub.Consumer
{
    class Program
    {
        private static Container _container;

        static async Task Main(string[] args)
        {
            //await InitCosmosDb();
            await ServiceBusConsumer.Consume();
            await EventHubConsumer.Consume();


            Console.WriteLine("Hello World!");
        }

        private static async Task InitCosmosDb()
        {
            string databaseId = "EdgeElevator";
            string containerId = "State";
            string cosmosConnectionString = "";
            CosmosClient client = new CosmosClient(cosmosConnectionString, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Direct });
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseId);
            ContainerResponse container = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/id") { PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2 });
            _container = container.Container;
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
