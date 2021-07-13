using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class MerchantDeleteFunction
    {
        [FunctionName("merchantdelete")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "merchant/{id}")] HttpRequest req,
            string id, ILogger log)
        {
            var worker = new MerchantDeleteWorker(id);
            await worker.Run();

            return new OkObjectResult("");
        }
    }
}
