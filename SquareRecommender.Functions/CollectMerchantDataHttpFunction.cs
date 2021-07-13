using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class CollectMerchantDataHttpFunction
    {
        [FunctionName("collectmerchantdataget")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "merchant/{id}/collectdata")] HttpRequest req,
            string id, ILogger log)
        {
            var collectMerchantDataWorker = new CollectMerchantDataWorker(id);
            await collectMerchantDataWorker.Run();
        }
    }
}
