using CosmosBackupSample.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosBackupSample
{
    class Program
    {
        private static CosmosClient cosmosClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private static async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(
            CosmosClient cosmosClient,
            IConfiguration configuration)
        {
            string databaseName = configuration["DatabaseName"];
            string sourceContainer = configuration["SourceContainerName"];
            string leaseContainerName = configuration["LeaseContainerName"];
            string backupContainerName = configuration["BackupContainerName"];

            Container leaseContainer = cosmosClient.GetContainer(databaseName, leaseContainerName);
            ChangeFeedProcessor changeFeedProcessor = cosmosClient.GetContainer(databaseName, sourceContainer)
                .GetChangeFeedProcessorBuilder<Product>("cosmosBackupSample", HandleChangesAsync)
                .WithInstanceName("consoleHost")
                .WithLeaseContainer(leaseContainer)
                .Build();

            Console.WriteLine("Starting Change Feed Processor for Product Backups...");
            await changeFeedProcessor.StartAsync();
            Console.WriteLine("Product Backup started");
            return changeFeedProcessor;
        }

        private static async Task HandleChangesAsync(IReadOnlyCollection<Product> changes, CancellationToken cancellationToken)
        {
            Console.WriteLine("Backing up Product items");

            foreach (Product product in changes)
            {
                // TODO: Add product to backup container.
                await cosmosClient.GetContainer("", "").CreateItemAsync(product);
            }

            Console.WriteLine("Finished handling changes");
        }
    }
}
