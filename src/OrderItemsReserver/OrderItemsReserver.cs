using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs;

using Microsoft.Azure.WebJobs;

using Microsoft.Extensions.Logging;

namespace OrderItemsReserver
{
    /// <summary>
    /// Order Items Reserver function should be able to create Reservation JSON files in Azure Blob Storage 
    /// by communicating through Service Bus.
    /// </summary>
    public class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task Run(
            [ServiceBusTrigger("ordersqueue", Connection = "ServiceBusConnectionString")] string myQueueItem,
            [Blob("orders", Connection = "AzureBlobStorageConnectionString")] BlobContainerClient blobContainerClient,
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            try
            {
                if(GetEnvironmentVariable("IsServiceBroken") == "true")
                {
                    throw new Exception("Something goes wrong here...");
                }
                

                await blobContainerClient.CreateIfNotExistsAsync();
                var jsonToUpload = myQueueItem;
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonToUpload));
                await blobContainerClient.UploadBlobAsync("order-" + Guid.NewGuid().ToString() + ".json", stream);
            } catch (Exception ex)
            {
                PostMessage(GetEnvironmentVariable("LogicAppUrl"), myQueueItem);
            }
        }

        private static void PostMessage(string url, string body)
        {
            using var httpClient = new HttpClient();
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(url, content).Result;
        }

        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
