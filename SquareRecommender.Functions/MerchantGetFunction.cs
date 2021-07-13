using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SquareRecommender.Db;

namespace SquareRecommender.Functions
{
    public static class MerchantGetFunction
    {
        [FunctionName("merchantget")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "merchant/{id}")] HttpRequest req,
            string id, ILogger log)
        {
            var merchantRepository = new MerchantRepository();
            var merchant = merchantRepository.GetMerchant(id);

            if (merchant == null)
                return new NotFoundObjectResult("Merchant not found");

            return new OkObjectResult(new
            {
                MerchantId = merchant.MerchantId,
                RecommendationsAvailable = merchant.RecommendationsAvailable
            });
        }
    }
}
