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
using System.Collections.Generic;
using SquareRecommender.Models;
using SquareRecommender.Workers;

namespace SquareRecommender.Functions
{
    public static class JobStatusesGetFunction
    {
        [FunctionName("jobstatuses")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "merchant/{id}/jobstatuses")] HttpRequest req,
            string id, ILogger log)
        {
            var merchantRepository = new MerchantRepository();
            var merchant = merchantRepository.GetMerchant(id);

            if (merchant == null)
                return new NotFoundObjectResult("Merchant not found");

            var jobStatusesGetWorker = new JobStatusesGetWorker(id);
            List<JobStatus> jobStatuses = jobStatusesGetWorker.Run();

            return new OkObjectResult(jobStatuses);
        }
    }
}
