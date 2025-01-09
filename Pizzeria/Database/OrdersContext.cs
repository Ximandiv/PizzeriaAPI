using MongoDB.Driver;
using Pizzeria.Database.Models;

namespace Pizzeria.Database
{
    public class OrdersContext
    {
        public IMongoCollection<Order> _orders;

        public OrdersContext(IMongoDatabase mongoDB)
        {
            _orders = mongoDB.GetCollection<Order>("Orders");

            var indexes = _orders.Indexes.List().ToList();
            bool indexExists = indexes.Any(i => i["name"].AsString == "userid_1");

            if (!indexExists)
            {
                var indexKeys = Builders<Order>.IndexKeys.Ascending(o => o.UserId);
                var indexOpts = new CreateIndexOptions { Background = true };
                _orders.Indexes.CreateOne(new CreateIndexModel<Order>(indexKeys, indexOpts));
            }
        }

        public async Task<List<Order>?> GetAllFromUser(int userId)
            => await _orders.Find(o => o.UserId == userId).ToListAsync();

        public async Task<Order?> GetOneFromUser(int userId, string orderId)
            => await _orders.Find(o => o.UserId == userId && o.Id == orderId).FirstOrDefaultAsync();

        public async Task Create(Order order)
            => await _orders.InsertOneAsync(order);

        public async Task CreateMany(List<Order> orderList)
            => await _orders.InsertManyAsync(orderList, new InsertManyOptions { IsOrdered = false });

        public async Task<long> Update(Order order, string orderId, int userId)
        {
            var result = await _orders.ReplaceOneAsync(o => o.Id == orderId && o.UserId == userId, order);
            return result.ModifiedCount;
        }

        public async Task<long> UpdateMany(List<Order> orderList, int userId)
        {
            var updateTasks = new List<Task<UpdateResult>>();
            foreach (var orderModel in orderList)
            {
                var order = await _orders.Find(o => o.UserId == userId && o.Id == orderModel.Id).FirstOrDefaultAsync();

                if (order is null) continue;

                orderModel.UpdateFromModel(order);

                var filter = Builders<Order>.Filter.And(
                    Builders<Order>.Filter.Eq(o => o.Id, orderModel.Id),
                    Builders<Order>.Filter.Eq(o => o.UserId, userId)
                    );
                var update = Builders<Order>.Update.Set(o => o.Items, orderModel.Items);
                updateTasks.Add(_orders.UpdateOneAsync(filter, update));
            }

            await Task.WhenAll(updateTasks);

            var modifiedCount = updateTasks.Sum(t => t.Result.ModifiedCount);

            return modifiedCount;
        }

        public async Task<bool> Delete(string orderId, int userId)
        {
            var result = await _orders.DeleteOneAsync(o => o.Id == orderId && o.UserId == userId);

            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteMany(List<string> orderId, int maxCount)
        {
            var filter = Builders<Order>.Filter.In(o => o.Id, orderId);
            var result = await _orders.DeleteManyAsync(filter);

            return result.DeletedCount == maxCount;
        }
    }
}
