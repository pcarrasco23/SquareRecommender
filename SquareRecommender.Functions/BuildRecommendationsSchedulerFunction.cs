using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SquareRecommender.Db;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class BuildRecommendationsSchedulerFunction
    {
        [FunctionName("buildrecommendationsschedulerfunction")]
        public static async Task Run([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var buildRecommendationsSchedulerWorker = new BuildRecommendationsSchedulerWorker();
            await buildRecommendationsSchedulerWorker.Run();
        }
    }
}
