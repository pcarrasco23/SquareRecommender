using MongoDB.Driver;
using System.Threading.Tasks;

namespace SquareRecommender.Db
{
    public class Datastore
    {
        public MongoClient Client;
        public IMongoDatabase Database;
        public readonly string databaseName;

        public Datastore()
        {
            initClient();
        }

        public Datastore(string databaseName)
        {
            initClient();

            this.databaseName = databaseName;
            this.Database = this.Client.GetDatabase(databaseName);
        }

        private void initClient()
        {
            var connectionString = System.Environment.GetEnvironmentVariable("DbConnectionString");

            var settings = MongoClientSettings.FromConnectionString(connectionString);

            this.Client = new MongoClient(settings);
        }

        public async Task RemoveMerchantData()
        {
            await this.Database.DropCollectionAsync("JobStatus");
            await this.Database.DropCollectionAsync("ProductIndex");
            await this.Database.DropCollectionAsync("ProductRecommendations");
        }
    }
}
