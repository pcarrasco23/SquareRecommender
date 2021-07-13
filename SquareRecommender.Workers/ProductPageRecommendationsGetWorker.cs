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
    public class ProductPageRecommendationsGetWorker
    {
        private readonly ProductIndexRepository productIndexRepository;
        private readonly string merchantId;
        private readonly string productPage;

        public ProductPageRecommendationsGetWorker(string productPage)
        {
            var domain = new Uri(productPage).Host;

            var merchantRepository = new MerchantRepository();
            var merchant = merchantRepository.GetMerchantByDomain(domain);

            if (merchant == null)
                throw new Exception("Merchant not found");

            var dataStore = new Datastore(merchant.MerchantId);
            this.productIndexRepository = new ProductIndexRepository(dataStore);

            this.merchantId = merchant.MerchantId;
            this.productPage = productPage;
        }

        public  async Task<List<EcommerceRecommendation>> Run()
        {
            var productIndex = this.productIndexRepository.GetProductIndexFromUrl(this.productPage);

            if (productIndex != null)
            {
                var productRecommenderworker = new ProductRecommendationsGetWorker(this.merchantId, productIndex.Id);

                return await productRecommenderworker.Run();
            }

            return new List<EcommerceRecommendation>();
        }
    }
}
