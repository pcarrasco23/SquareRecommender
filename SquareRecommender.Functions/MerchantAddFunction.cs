using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SquareRecommender.Db;
using SquareRecommender.Models;
using Azure.Storage.Queues;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class MerchantAddFunction
    {
        [FunctionName("merchantadd")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "merchant")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var merchantAddWorker = new MerchantAddWorker(data);
            await merchantAddWorker.Run();

            return new OkObjectResult("");
        }
    }
}
