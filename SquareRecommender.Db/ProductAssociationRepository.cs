using SquareRecommender.Models;

namespace SquareRecommender.Db
{
    public class ProductAssociationRepository : Repository<ProductAssociation>
    {
        public ProductAssociationRepository(Datastore database) : base(database, "ProductAssociation")
        {
        }
    }
}
