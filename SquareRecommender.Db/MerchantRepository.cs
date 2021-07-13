using System;
using System.Linq;
using System.Threading.Tasks;
using SquareRecommender.Models;
using MongoDB.Driver;

namespace SquareRecommender.Db
{
    public class MerchantRepository : Repository<Merchant>
    {
        public MerchantRepository() : base(new Datastore("squarerecommenderadmin"), "merchant")
        {
        }

        public Merchant GetMerchant(string merchantId)
        {
            var filter = Builders<Merchant>.Filter.Eq(n => n.MerchantId, merchantId);

            return this.table.Find<Merchant>(filter).FirstOrDefault();
        }

        public Merchant GetMerchantByDomain(string domain)
        {
            var filter = Builders<Merchant>.Filter.Eq(n => n.Domain, domain.ToLower());

            return this.table.Find<Merchant>(filter).FirstOrDefault();
        }

        public void UpdateRecommendationsAvailable(string merchantId, bool available)
        {
            var filter = Builders<Merchant>.Filter.Eq(n => n.MerchantId, merchantId);
            var update = Builders<Merchant>.Update.Set("RecommendationsAvailable", available);
            
            this.table.UpdateOne(filter, update);
        }
    }
}
