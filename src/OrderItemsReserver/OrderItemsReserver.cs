using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("orders", Connection = "CONNECTION_STRING_AZURE_BLOB")] BlobContainerClient blobContainerClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.UploadBlobAsync("order-" + Guid.NewGuid().ToString() + ".json", req.Body);

            return new OkObjectResult("Success");
        }
    }
}
