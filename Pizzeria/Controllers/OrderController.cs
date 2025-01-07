using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Pizzeria.Database;
using Pizzeria.Database.Models;
using Pizzeria.DTOs;

namespace Pizzeria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrdersContext _ordersContext;

        public OrderController(OrdersContext ordersContext)
        {
            _ordersContext = ordersContext;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllFromUser(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid User ID");

            var orders = await _ordersContext.GetAllFromUser(userId);

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

            var order = await _ordersContext.GetOneFromUser(userId, orderId);

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
                await _ordersContext.Create(model);
            }
            catch(Exception)
            {
                return StatusCode(500, "Error found while creating entity");
            }

            var response = new OrderResponseDTO(model);

            return CreatedAtAction(nameof(Create), $"api/order/{response.OrderId}/user/{response.UserId}", response);
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
                await _ordersContext.CreateMany(modelList);
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

            var orderModel = await _ordersContext.GetOneFromUser(userId, orderId);

            if (orderModel is null) return NotFound();

            model.UpdateFromModel(orderModel);

            try
            {
                var hadUpdateChanges = await _ordersContext.Update(model, orderId, userId);

                if (!hadUpdateChanges) return BadRequest("No changes done in update");
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
                var amountToUpdate = modelList.Count;
                var hadModifications = await _ordersContext.UpdateMany(modelList, userId, amountToUpdate);

                if (!hadModifications) return BadRequest("One or more orders were not modified");
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

            var wasDeleted = await _ordersContext.Delete(orderId, userId);

            if (!wasDeleted) return BadRequest("No order was deleted");

            return NoContent();
        }

        [HttpDelete("many/User/{userId}")]
        public async Task<IActionResult> DeleteMany(List<string> orderId)
        {
            var validIds = orderId.Any(id => id is not null);

            if (!validIds) return BadRequest("Invalid Order IDs");

            var amountToDelete = orderId.Count;
            var filter = Builders<Order>.Filter.In(o => o.Id, orderId);
            var wereDeleted = await _ordersContext.DeleteMany(orderId, amountToDelete);

            if (!wereDeleted) return BadRequest("One or more orders were not deleted");

            return NoContent();
        }
    }
}
