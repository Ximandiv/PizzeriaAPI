using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Pizzeria.Database.Models;
using Pizzeria.DTOs;

namespace Pizzeria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderController(IMongoDatabase mongoDB)
        {
            _orders = mongoDB.GetCollection<Order>("Orders");

            var indexKeys = Builders<Order>.IndexKeys.Ascending(o => o.UserId);
            var indexOpts = new CreateIndexOptions { Background = true };
            _orders.Indexes.CreateOne(new CreateIndexModel<Order>(indexKeys, indexOpts));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllFromUser(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid User ID");

            var orders = await _orders.Find(o => o.UserId == userId).ToListAsync();

            if(orders is null
                || orders.Count == 0)
                return NotFound();

            return Ok(orders);
        }

        [HttpGet("{orderId}/user/{userId}")]
        public async Task<IActionResult> GetOneFromUser(string orderId, int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid User ID");

            var order = await _orders.Find(o => o.UserId == userId && o.Id == orderId).FirstOrDefaultAsync();

            if(order is null) return NotFound();

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderRequestDTO order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (order.Items.Count == 0) return BadRequest("No Items found in Order");

            var model = order.ToModel();

            if (model is null)
                return StatusCode(500, "Error found parsing to entity");

            try
            {
                await _orders.InsertOneAsync(model);
            }
            catch(Exception)
            {
                return StatusCode(500, "Error found while creating entity");
            }

            var response = new OrderResponseDTO(model);

            return Ok(response);
        }

        [HttpPost("many")]
        public async Task<IActionResult> CreateMany(List<OrderRequestDTO> orderList)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validOrderItems = orderList.Sum(o => o.Items.Count) > 0;

            if (!validOrderItems) return BadRequest("No Items found in an Order");

            var modelList = orderList.Select(o => o.ToModel()).ToList();

            if (modelList is null
                || modelList.Count == 0) return StatusCode(500, "Error found parsing to entity");

            try
            {
                await _orders.InsertManyAsync(modelList);
            }
            catch(Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            var responseList = new List<OrderResponseDTO>();
            foreach(var orderModel in modelList)
            {
                var responseModel = new OrderResponseDTO(orderModel);
                responseList.Add(responseModel);
            }

            return Ok(responseList);
        }

        [HttpPut("{orderId}/User/{userId}")]
        public async Task<IActionResult> UpdateOne(OrderRequestDTO order, string orderId, int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (order.Items.Count == 0) return BadRequest("No Items found in Order");

            order.UserId = userId;

            var model = order.ToModel();

            if (model is null)
                return StatusCode(500, "Error found parsing to entity");

            var orderModel = await _orders.Find(o => o.UserId == userId && o.Id == orderId).FirstOrDefaultAsync();

            if (orderModel is null) return NotFound();

            model.CreatedAt = orderModel.CreatedAt;
            model.Id = orderModel.Id;
            model.UserId = userId;

            try
            {
                var result = await _orders.ReplaceOneAsync(o => o.Id == orderId && o.UserId == userId, model);

                if (result.ModifiedCount == 0) return BadRequest("No changes done in update");
            }
            catch(Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            return NoContent();
        }

        [HttpPut("many/User/{userId}")]
        public async Task<IActionResult> UpdateMany(List<OrderUpdateDTO> orderUpdateList, int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validModel = orderUpdateList.Any(o => o.Order is not null && o.Order.Items.Count > 0);

            if (!validModel)
                return BadRequest("No Items found in Order");

            List<Order> modelList = new();

            foreach(var order in orderUpdateList)
            {
                var orderModel = order.Order!.ToModel();
                orderModel.Id = order.OrderId!;

                modelList.Add(orderModel);
            }

            if(modelList.Count == 0) return StatusCode(500, "Error found while parsing to entities");

            var updateTasks = new List<Task<UpdateResult>>();

            try
            {
                foreach (var orderModel in modelList)
                {
                    var order = await _orders.Find(o => o.UserId == userId && o.Id == orderModel.Id).FirstOrDefaultAsync();

                    orderModel.Id = order.Id;
                    orderModel.UserId = userId;
                    orderModel.CreatedAt = order.CreatedAt;

                    var filter = Builders<Order>.Filter.And(
                        Builders<Order>.Filter.Eq(o => o.Id, orderModel.Id),
                        Builders<Order>.Filter.Eq(o => o.UserId, userId)
                        );
                    var update = Builders<Order>.Update.Set(o => o.Items, orderModel.Items);
                    updateTasks.Add(_orders.UpdateOneAsync(filter, update));
                }

                await Task.WhenAll(updateTasks);

                var modifiedCount = updateTasks.Sum(t => t.Result.ModifiedCount);

                if (modifiedCount == 0) return BadRequest("No orders were updated");
            }
            catch(Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            return NoContent();
        }

        [HttpDelete("{orderId}/User/{userId}")]
        public async Task<IActionResult> DeleteOne(string orderId, int userId)
        {
            if (userId == 0) return BadRequest("Invalid user ID");

            var result = await _orders.DeleteOneAsync(o => o.Id == orderId && o.UserId == userId);

            if (result.DeletedCount == 0) return BadRequest("No orders were deleted");

            return NoContent();
        }

        [HttpDelete("many/User/{userId}")]
        public async Task<IActionResult> DeleteMany(List<string> orderId)
        {
            var validIds = orderId.Any(id => id is not null);

            if (!validIds) return BadRequest("Invalid Order IDs");

            var filter = Builders<Order>.Filter.In(o => o.Id, orderId);
            var result = await _orders.DeleteManyAsync(filter);

            if (result.DeletedCount == 0) return BadRequest("No orders were deleted");

            return NoContent();
        }
    }
}
