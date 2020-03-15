using CosmosBackupSample.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosBackupSample
{
    class Program
    {
        private static CosmosClient _cosmosClient;

        private static Database _productDatabase;
        private static Container _productContainer;
        private static Container _productLeaseContainer;
        private static Container _productBackUpContainer;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Change Feed Sample");

            _cosmosClient = new CosmosClient("");
            _productDatabase = _cosmosClient.GetDatabase("Products");
            _productContainer = _productDatabase.GetContainer("Product");
            _productLeaseContainer = _productDatabase.GetContainer("lease");
            _productBackUpContainer = _productDatabase.GetContainer("ProductBackup");

            try
            {
                await StartChangeFeedProcessorAsync(_cosmosClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong. Exception thrown: {ex.Message}");
                throw;
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(
            CosmosClient cosmosClient)
        {
            Container leaseContainer = cosmosClient.GetContainer(_productDatabase.Id, _productLeaseContainer.Id);
            ChangeFeedProcessor changeFeedProcessor = cosmosClient.GetContainer(_productDatabase.Id, _productContainer.Id)
                .GetChangeFeedProcessorBuilder<Product>("cosmosBackupSample", HandleChangesAsync)
                .WithInstanceName("consoleHost")
                .WithLeaseContainer(leaseContainer)
                .Build();

            Console.WriteLine("Starting Change Feed Processor for Product Backups...");
            await changeFeedProcessor.StartAsync();
            Console.WriteLine("Product Backup started");
            return changeFeedProcessor;
        }

        private static async Task HandleChangesAsync(
            IReadOnlyCollection<Product> changes,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("Backing up Product items");

            foreach (Product product in changes)
            {
                await _productBackUpContainer.CreateItemAsync(
                    product,
                    partitionKey: new PartitionKey(product.ProductCategory));
                Console.WriteLine($"Product added to backup container");
                await Task.Delay(10);
            }

            Console.WriteLine("Finished handling changes");
        }
    }
}
