using Newtonsoft.Json.Linq;
using Square;
using SquareRecommender.Db;
using SquareRecommender.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Square.Models;

namespace SquareRecommender.Workers
{
    public class ProductRecommendationsGetWorker
    {
        private readonly ProductRecommendationsRepository productRecommendationsRepository;
        private readonly ProductIndexRepository productIndexRepository;
        private readonly string productId;
        private readonly SquareClient squareClient;

        public ProductRecommendationsGetWorker(string merchantId, string productId)
        {
            var dataStore = new Datastore(merchantId);
            this.productRecommendationsRepository = new ProductRecommendationsRepository(dataStore);
            this.productIndexRepository = new ProductIndexRepository(dataStore);

            var merchantRepository = new MerchantRepository();
            var merchant = merchantRepository.GetMerchant(merchantId);

            var squareEnvironment = System.Environment.GetEnvironmentVariable("SquareEnvironment");
            var env = squareEnvironment == "production" ? Square.Environment.Production : Square.Environment.Sandbox;

            this.squareClient = new SquareClient.Builder()
                .Environment(env)
                .AccessToken(merchant.AccessToken)
                .Build();

            this.productId = productId;
        }

        public  async Task<List<EcommerceRecommendation>> Run()
        {
            var productRecommendations = this.productRecommendationsRepository.GetProductRecommendations(this.productId);
            var recommendations = productRecommendations.Recommendations.Split(',');

            var ecommerceRecommendations = new List<EcommerceRecommendation>();

            foreach (var recommendation in recommendations)
            {
                var productIndex = await this.productIndexRepository.GetItem(recommendation);

                ecommerceRecommendations.Add(new EcommerceRecommendation
                {
                    ProductId = recommendation,
                    Name = productIndex.ProductName,
                    PageUrl = productIndex.PageUrl,
                    ImageUrl = productIndex.ImageUrl
                });
            }

            return ecommerceRecommendations;
        }
    }
}
