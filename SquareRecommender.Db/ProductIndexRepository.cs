using MongoDB.Driver;
using SquareRecommender.Models;

namespace SquareRecommender.Db
{
    public class ProductIndexRepository : Repository<ProductIndex>
    {
        public ProductIndexRepository(Datastore database) : base(database, "ProductIndex")
        {
        }

        public ProductIndex GetProductIndexFromUrl(string productUrl)
        {
            productUrl = FormatProductUrl(productUrl);

            var filter = Builders<ProductIndex>.Filter.Eq(n => n.PageUrl, productUrl);

            return this.table.Find<ProductIndex>(filter).FirstOrDefault();
        }

        private string FormatProductUrl(string productUrl)
        {
            if (productUrl.Contains("?"))
                productUrl = productUrl.Substring(0, productUrl.IndexOf("?"));

            return productUrl;
        }
    }
}
