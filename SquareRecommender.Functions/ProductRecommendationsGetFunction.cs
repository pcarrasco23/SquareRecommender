using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class ProductRecommendationsGetFunction
    {
        [FunctionName("productrecommendationsget")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "merchant/{merchantId}/product/{productId}/recommendations")] HttpRequest req,
            string merchantId, string productId, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var productRecommendationsGetWorker = new ProductRecommendationsGetWorker(merchantId, productId);
            var productRecommendations = await productRecommendationsGetWorker.Run();

            return new OkObjectResult(productRecommendations);
        }
    }
}
