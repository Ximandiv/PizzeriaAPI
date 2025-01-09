using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Pizzeria.Database;
using Pizzeria.Database.Models;
using Pizzeria.DTOs.Orders;
using Pizzeria.Services;
using System.Security.Claims;

namespace Pizzeria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrdersContext _ordersContext;
        private readonly UserService _userService;

        public OrderController(OrdersContext ordersContext, UserService userService)
        {
            _ordersContext = ordersContext;
            _userService = userService;
        }

        [HttpGet("user/me")]
        [Authorize]
        public async Task<IActionResult> GetAllFromMe()
        {
            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if(user is null) return Forbid();

            var orders = await _ordersContext.GetAllFromUser(user.Id);

            if (orders is null
                || orders.Count == 0)
                return NotFound();

            return Ok(orders);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin")]
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


        [HttpGet("{orderId}/user/me")]
        [Authorize]
        public async Task<IActionResult> GetOneFromMe(string orderId)
        {
            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if (user is null) return Forbid();

            var order = await _ordersContext.GetOneFromUser(user.Id, orderId);

            if (order is null) return NotFound();

            return Ok(order);
        }

        [HttpGet("{orderId}/user/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetOneFromUser(string orderId, int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid User ID");

            var order = await _ordersContext.GetOneFromUser(userId, orderId);

            if(order is null) return NotFound();

            return Ok(order);
        }

        [HttpPost("user/me")]
        [Authorize]
        public async Task<IActionResult> CreateFromMe(OrderRequestMeDTO order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if (user is null) return Forbid();

            if (order.Items.Count == 0) return BadRequest("No Items found in Order");

            var model = order.ToModel();

            if (model is null)
                return StatusCode(500, "Error found parsing to entity");

            model.UserId = user.Id;

            try
            {
                await _ordersContext.Create(model);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error found while creating entity");
            }

            var response = new OrderResponseDTO(model);

            return CreatedAtAction(nameof(GetOneFromMe), new { orderId = response.OrderId }, response);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
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

            return CreatedAtAction(nameof(GetOneFromUser), new { orderId = response.OrderId, userId = response.UserId }, response);
        }

        [HttpPost("many/user/me")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateManyFromMe(List<OrderRequestMeDTO> orderList)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if (user is null) return Forbid();

            var validOrderItems = orderList.Sum(o => o.Items.Count) > 0;

            if (!validOrderItems) return BadRequest("No Items found in an Order");

            var modelList = orderList.Select(o => o.ToModel()).ToList();
            modelList.ForEach(o => o.UserId = user.Id);

            if (modelList is null
                || modelList.Count == 0) return StatusCode(500, "Error found parsing to entity");

            int unsuccessfulInserts = 0;
            try
            {
                await _ordersContext.CreateMany(modelList);
            }
            catch (MongoBulkWriteException<Order> ex)
            {
                unsuccessfulInserts = ex.WriteErrors.Count;

                if (unsuccessfulInserts == modelList.Count)
                    return StatusCode(500, "Error found while creating entities");
            }
            catch (Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            var responseList = new List<OrderResponseDTO>();
            foreach (var orderModel in modelList)
            {
                var responseModel = new OrderResponseDTO(orderModel);
                responseList.Add(responseModel);
            }

            if (unsuccessfulInserts > 0)
                return StatusCode(207, responseList);

            return Ok(responseList);
        }

        [HttpPost("many")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateMany(List<OrderRequestDTO> orderList)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validOrderItems = orderList.Sum(o => o.Items.Count) > 0;

            if (!validOrderItems) return BadRequest("No Items found in an Order");

            var modelList = orderList.Select(o => o.ToModel()).ToList();

            if (modelList is null
                || modelList.Count == 0) return StatusCode(500, "Error found parsing to entity");

            int unsuccessfulInserts = 0;
            try
            {
                await _ordersContext.CreateMany(modelList);
            }
            catch(MongoBulkWriteException<Order> ex)
            {
                unsuccessfulInserts = ex.WriteErrors.Count;

                if(unsuccessfulInserts == modelList.Count)
                    return StatusCode(500, "Error found while creating entities");
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

            if (unsuccessfulInserts > 0)
                return StatusCode(207, responseList);

            return Ok(responseList);
        }

        [HttpPut("{orderId}/User/me")]
        [Authorize]
        public async Task<IActionResult> UpdateOneFromMe(OrderRequestDTO order, string orderId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if (user is null) return Forbid();

            if (order.Items.Count == 0) return BadRequest("No Items found in Order");

            var orderModel = await _ordersContext.GetOneFromUser(user.Id, orderId);

            if (orderModel is null) return NotFound();

            order.UserId = user.Id;

            var model = order.ToModel();

            if (model is null)
                return StatusCode(500, "Error found parsing to entity");

            model.UpdateFromModel(orderModel);

            try
            {
                var amountUpdateChanges = await _ordersContext.Update(model, orderId, user.Id);

                if (amountUpdateChanges == 0) return BadRequest("No changes done in update");
            }
            catch (Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            return NoContent();
        }

        [HttpPut("{orderId}/User/{userId}")]
        [Authorize(Roles = "admin")]
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
                var amountUpdateChanges = await _ordersContext.Update(model, orderId, userId);

                if (amountUpdateChanges == 0) return BadRequest("No changes done in update");
            }
            catch(Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            return NoContent();
        }

        [HttpPut("many/User/me")]
        [Authorize]
        public async Task<IActionResult> UpdateManyFromMe(List<OrderUpdateDTO> orderUpdateList)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if (user is null) return Forbid();

            var validModel = orderUpdateList.Any(o => o.Order is not null && o.Order.Items.Count > 0 && !string.IsNullOrEmpty(o.OrderId));

            if (!validModel)
                return UnprocessableEntity("No Items found in Order");

            List<Order> modelList = new();

            foreach (var order in orderUpdateList)
            {
                var originalOrder = await _ordersContext.GetOneFromUser(user.Id, order.OrderId!);

                if (originalOrder is null)
                    return UnprocessableEntity(new { Message = "An order was not found", Object = order });

                var orderModel = order.Order!.ToModel();
                orderModel.Id = order.OrderId!;

                modelList.Add(orderModel);
            }

            if (modelList.Count == 0) return StatusCode(500, "Error found while parsing to entities");

            var updateTasks = new List<Task<UpdateResult>>();

            long modificationsTotal = 0;
            try
            {
                modificationsTotal = await _ordersContext.UpdateMany(modelList, user.Id);

                if (modificationsTotal == 0) return StatusCode(207, "One or more orders were not modified");
            }
            catch (Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            if (modificationsTotal < modelList.Count)
                return StatusCode(207);

            return NoContent();
        }

        [HttpPut("many/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateMany(List<OrderUpdateDTO> orderUpdateList, int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validModel = orderUpdateList.Any(o => o.Order is not null && o.Order.Items.Count > 0);

            if (!validModel)
                return UnprocessableEntity("No Items found in Order");

            List<Order> modelList = new();

            foreach(var order in orderUpdateList)
            {
                var originalOrder = await _ordersContext.GetOneFromUser(userId, order.OrderId!);

                if (originalOrder is null)
                    return UnprocessableEntity(new { Message = "An order was not found", Object = order });

                var orderModel = order.Order!.ToModel();
                orderModel.Id = order.OrderId!;

                modelList.Add(orderModel);
            }

            if(modelList.Count == 0) return StatusCode(500, "Error found while parsing to entities");

            var updateTasks = new List<Task<UpdateResult>>();

            long modificationsTotal = 0;
            try
            {
                modificationsTotal = await _ordersContext.UpdateMany(modelList, userId);

                if (modificationsTotal == 0) return BadRequest("No orders were modified");
            }
            catch (Exception)
            {
                return StatusCode(500, "Error found while creating entities");
            }

            if (modificationsTotal < modelList.Count)
                return StatusCode(207);

            return NoContent();
        }

        [HttpDelete("{orderId}/User/me")]
        [Authorize]
        public async Task<IActionResult> DeleteOneFromMe(string orderId)
        {
            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if (user is null) return Forbid();

            var wasDeleted = await _ordersContext.Delete(orderId, user.Id);

            if (!wasDeleted) return BadRequest("No order was deleted");

            return NoContent();
        }

        [HttpDelete("{orderId}/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOne(string orderId, int userId)
        {
            if (userId == 0) return BadRequest("Invalid user ID");

            var wasDeleted = await _ordersContext.Delete(orderId, userId);

            if (!wasDeleted) return BadRequest("No order was deleted");

            return NoContent();
        }

        [HttpDelete("many/User/me")]
        [Authorize]
        public async Task<IActionResult> DeleteManyFromMe(List<string> orderId)
        {
            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            var user = await _userService.GetByEmail(authUserEmail);

            if (user is null) return Forbid();

            var validIds = !orderId.Any(string.IsNullOrEmpty);

            if (!validIds) return BadRequest("Invalid Order IDs");

            var amountToDelete = orderId.Count;
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.In(o => o.Id, orderId),
                Builders<Order>.Filter.Eq(o => o.UserId, user.Id)
                );
            var wereDeleted = await _ordersContext.DeleteMany(orderId, amountToDelete);

            if (!wereDeleted) return BadRequest("One or more orders were not deleted");

            return NoContent();
        }

        [HttpDelete("many/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMany(List<string> orderId, int userId)
        {
            var validIds = orderId.Any(id => id is not null);

            if (!validIds) return BadRequest("Invalid Order IDs");

            var amountToDelete = orderId.Count;
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.In(o => o.Id, orderId),
                Builders<Order>.Filter.Eq(o => o.UserId, userId)
                );
            var wereDeleted = await _ordersContext.DeleteMany(orderId, amountToDelete);

            if (!wereDeleted) return BadRequest("One or more orders were not deleted");

            return NoContent();
        }
    }
}
