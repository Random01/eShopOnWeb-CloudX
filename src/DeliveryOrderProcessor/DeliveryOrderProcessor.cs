using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DeliveryOrderProcessor
{
    /// <summary>
    /// Delivery Order Processor functions should be triggered after the order has been created 
    /// and send the order detail information to CosmosDB.
    /// </summary>
    public static class DeliveryOrderProcessor
    {
        [FunctionName("DeliveryOrderProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "orders-db",
                containerName: "orders",
                Connection = "CosmosDbConnectionString")] IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Add a JSON document to the output container.
            await documentsOut.AddAsync(new
            {
                // create a random ID
                id = Guid.NewGuid().ToString(),
                order = data
            });

            return new OkObjectResult("Success");
        }
    }
}
