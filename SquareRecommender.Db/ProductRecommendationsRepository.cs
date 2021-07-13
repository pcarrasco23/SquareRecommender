using MongoDB.Driver;
using SquareRecommender.Models;
using System.Linq;

namespace SquareRecommender.Db
{
    public class ProductRecommendationsRepository : Repository<ProductRecommendations>
    {
        public ProductRecommendationsRepository(Datastore database) : base(database, "ProductRecommendations")
        {
        }

        public ProductRecommendations GetProductRecommendations(string productId)
        {
            var filter = Builders<ProductRecommendations>.Filter.Eq(n => n.ProductId, productId);

            return this.table.Find<ProductRecommendations>(filter).FirstOrDefault();
        }
    }
}
