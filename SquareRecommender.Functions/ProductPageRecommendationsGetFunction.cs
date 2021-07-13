using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SquareRecommender.Workers;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SquareRecommender.Functions
{
    public static class ProductPageRecommendationsGetFunction
    {
        [FunctionName("productpagerecommendationsget")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "recommendations")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            StreamReader reader = new StreamReader(req.Body);
            string body = reader.ReadToEnd();
            JObject json = JObject.Parse(body);
            var url = json.SelectToken("url");

            var productPageRecommendationsGetWorker = new ProductPageRecommendationsGetWorker(url.ToString());
            var productRecommendations = await productPageRecommendationsGetWorker.Run();

            return new OkObjectResult(productRecommendations);
        }
    }
}
