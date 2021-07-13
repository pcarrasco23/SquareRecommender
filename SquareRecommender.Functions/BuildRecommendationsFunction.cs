using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class BuildRecommendationsFunction
    {
        [FunctionName("buildrecommendations")]
        public static async Task Run([QueueTrigger("buildrecommendations", Connection = "SquareRecommenderQueue")]string queueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");

            var buildRecommendationsWorker = new BuildRecommendationsWorker(queueItem);
            await buildRecommendationsWorker.Run();
        }
    }
}
