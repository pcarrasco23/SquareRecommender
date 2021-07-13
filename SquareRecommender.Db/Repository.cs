using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace SquareRecommender.Db
{
    public class Repository<T>
    {
        protected readonly IMongoCollection<T> table;

        public Repository(Datastore dataStore, string tableName)
        {
            this.table = dataStore.Database.GetCollection<T>(tableName);
        }

        public async Task<T> GetItem(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            var cursor = await this.table.FindAsync<T>(filter);

            return cursor.FirstOrDefault();
        }

        public async Task AddItem(T item)
        {
            await this.table.InsertOneAsync(item);
        }

        public List<T> GetAll()
        {
            return this.table.Find(Builders<T>.Filter.Empty).ToList();
        }

        public void RemoveAll()
        {
            var items = this.table.DeleteMany(_ => true);
        }

        public void RemoveItem(string id)
        {
            var deleteFilter = Builders<T>.Filter.Eq("_id", id);
            this.table.DeleteOne(deleteFilter);
        }

        protected virtual string IdValue(T item)
        {
            return "";
        }
    }
}
