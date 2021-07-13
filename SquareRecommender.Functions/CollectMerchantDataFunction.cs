using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class CollectMerchantDataFunction
    {
        [FunctionName("collectmerchantdata")]
        public static async Task Run([QueueTrigger("collectmerchantdata", Connection = "SquareRecommenderQueue")]string queueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");

            var collectMerchantDataWorker = new CollectMerchantDataWorker(queueItem);
            await collectMerchantDataWorker.Run();
        }
    }
}
