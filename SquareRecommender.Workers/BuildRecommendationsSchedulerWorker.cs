using Azure.Storage.Queues;
using SquareRecommender.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SquareRecommender.Workers
{
    public class BuildRecommendationsSchedulerWorker
    {
        public BuildRecommendationsSchedulerWorker()
        {

        }

        public async Task Run()
        {
            string queueConnectionString = Environment.GetEnvironmentVariable("SquareRecommenderQueue");

            var merchantRepository = new MerchantRepository();
            var merchants = merchantRepository.GetAll();

            foreach (var merchant in merchants)
            {
                if (merchant.Enabled)
                {
                    var queue = new QueueClient(queueConnectionString, "collectmerchantdata", new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });
                    await queue.SendMessageAsync(merchant.MerchantId);
                }
            }
        }
    }
}
