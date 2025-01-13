using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Pizzeria.Database;
using Pizzeria.Database.Models;
using Pizzeria.DTOs;
using Pizzeria.DTOs.Orders;
using Pizzeria.DTOs.Users;
using Pizzeria.Services;
using System.Security.Claims;

namespace Pizzeria.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController
        (OrdersContext ordersContext,
        UserService userService,
        ILogger<OrderController> logger): ControllerBase
    {
        private readonly OrdersContext _ordersContext = ordersContext;
        private readonly UserService _userService = userService;
        private readonly ILogger<OrderController> _logger = logger;

        [HttpGet("user/me")]
        [Authorize]
        public async Task<IActionResult> GetAllFromMe()
        {
            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(GetAllFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userErrorResponse = UserError.GetByEmail;
                return StatusCode(500, userErrorResponse);
            }

            if(user is null) return Forbid();

            List<Order>? orders;
            try
            {
                orders = await _ordersContext.GetAllFromUser(user.Id);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(GetAllFromMe), DateTime.UtcNow, user.Id, ex);
                ResultObject<Error> orderErrorResponse = OrderError.GetAllFromUser;
                return StatusCode(500, orderErrorResponse);
            }

            if (orders is null
                || orders.Count == 0)
                return NotFound();

            ResultObject<List<OrderResponse>> response = orders.Select(o => o.ToDTO()).ToList();
            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllFromUser(int userId)
        {
            if (userId <= 0)
                return BadRequest(UserError.InvalidId);

            List<Order>? orders;
            try
            {
                orders = await _ordersContext.GetAllFromUser(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(GetAllFromUser), DateTime.UtcNow, userId, ex);
                ResultObject<Error> errorResponse = OrderError.GetAllFromUser;
                return StatusCode(500, errorResponse);
            }

            if(orders is null
                || orders.Count == 0)
                return NotFound();

            ResultObject<List<OrderResponse>> response = orders.Select(o => o.ToDTO()).ToList();
            return Ok(response);
        }


        [HttpGet("{orderId}/user/me")]
        [Authorize]
        public async Task<IActionResult> GetOneFromMe(string orderId)
        {
            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(GetOneFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userError = UserError.GetByEmail;
                return StatusCode(500, userError);
            }

            if (user is null) return Forbid();

            Order? order;
            try
            {
                order = await _ordersContext.GetOneFromUser(user.Id, orderId);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(GetOneFromMe), DateTime.UtcNow, orderId, ex);
                ResultObject<Error> orderErrorResponse = OrderError.GetOneFromUser;
                return StatusCode(500, orderErrorResponse);
            }

            if (order is null) return NotFound();

            ResultObject<OrderResponse> response = order.ToDTO();
            return Ok(response);
        }

        [HttpGet("{orderId}/user/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetOneFromUser(string orderId, int userId)
        {
            if (userId <= 0)
                return BadRequest(UserError.InvalidId);

            Order? order;
            try
            {
                order = await _ordersContext.GetOneFromUser(userId, orderId);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(GetOneFromUser), DateTime.UtcNow, new {orderId, userId}, ex);
                ResultObject<Error> errorResponse = OrderError.GetOneFromUser;
                return StatusCode(500, errorResponse);
            }

            if(order is null) return NotFound();

            ResultObject<OrderResponse> response = order.ToDTO();
            return Ok(response);
        }

        [HttpPost("user/me")]
        [Authorize]
        public async Task<IActionResult> CreateFromMe(OrderRequestMeDTO order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(CreateFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userError = UserError.GetByEmail;
                return StatusCode(500, userError);
            }

            if (user is null) return Forbid();

            if (order.Items.Count == 0)
            {
                ResultObject<Error> itemsResponse = OrderError.InvalidItems;
                return BadRequest(itemsResponse);
            }

            var model = order.ToModel();

            if (model is null
                || (model.Items is null || model.Items.Count == 0))
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            model.UserId = user.Id;

            try
            {
                await _ordersContext.Create(model);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(CreateFromMe), DateTime.UtcNow, order, ex);
                ResultObject<Error> errorResponse = OrderError.Create;
                return StatusCode(500, errorResponse);
            }

            ResultObject<OrderResponse> response = new OrderResponse(model);
            return CreatedAtAction(nameof(GetOneFromMe), new { orderId = model.Id }, response);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(OrderRequestDTO order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (order.Items.Count == 0)
            {
                var itemsResponse = OrderError.InvalidItems;
                return BadRequest(itemsResponse);
            }

            var model = order.ToModel();

            if (model is null)
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            try
            {
                await _ordersContext.Create(model);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(Create), DateTime.UtcNow, order, ex);
                ResultObject<Error> errorResponse = OrderError.Create;
                return StatusCode(500, errorResponse);
            }

            ResultObject<OrderResponse> response = new OrderResponse(model);
            return CreatedAtAction(nameof(GetOneFromUser), new { orderId = model.Id, userId = model.UserId }, response);
        }

        [HttpPost("many/user/me")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateManyFromMe(List<OrderRequestMeDTO> orderList)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                            nameof(OrderController), nameof(CreateManyFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userError = UserError.GetByEmail;
                return StatusCode(500, userError);
            }

            if (user is null) return Forbid();

            var validOrderItems = orderList.Sum(o => o.Items.Count) > 0;

            if (!validOrderItems)
            {
                ResultObject<Error> itemsResponse = OrderError.InvalidItems;
                return BadRequest(itemsResponse);
            }

            var modelList = orderList.Select(o => o.ToModel()).ToList();
            modelList.ForEach(o => o.UserId = user.Id);

            var validModelItems = modelList.Any(o => o.Items is not null || o.Items!.Count == 0);
            if (modelList is null
                || modelList.Count == 0
                || !validModelItems)
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            int unsuccessfulInserts = 0;
            try
            {
                await _ordersContext.CreateMany(modelList);
            }
            catch (MongoBulkWriteException<Order> ex)
            {
                unsuccessfulInserts = ex.WriteErrors.Count;

                if (unsuccessfulInserts == modelList.Count)
                {
                    _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(CreateManyFromMe), DateTime.UtcNow, orderList, ex);
                    ResultObject<Error> unsuccessfulResponse = OrderError.Create;
                    return StatusCode(500, unsuccessfulInserts);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(CreateManyFromMe), DateTime.UtcNow, orderList, ex);
                ResultObject<Error> errorResponse = OrderError.Create;
                return StatusCode(500, errorResponse);
            }

            var responseList = new List<OrderResponse>();
            foreach (var orderModel in modelList)
            {
                var responseModel = new OrderResponse(orderModel);
                responseList.Add(responseModel);
            }

            ResultObject<List<OrderResponse>> response = responseList;

            if (unsuccessfulInserts > 0)
                return StatusCode(207, response);

            return Ok(response);
        }

        [HttpPost("many")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateMany(List<OrderRequestDTO> orderList)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validOrderItems = orderList.Sum(o => o.Items.Count) > 0;

            if (!validOrderItems)
            {
                ResultObject<Error> itemsResponse = OrderError.InvalidItems;
                return BadRequest(itemsResponse);
            }

            var modelList = orderList.Select(o => o.ToModel()).ToList();

            var validModelItems = modelList.Any(o => o.Items is not null || o.Items!.Count == 0);
            if (modelList is null
                || modelList.Count == 0
                || !validModelItems)
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            int unsuccessfulInserts = 0;
            try
            {
                await _ordersContext.CreateMany(modelList);
            }
            catch(MongoBulkWriteException<Order> ex)
            {
                unsuccessfulInserts = ex.WriteErrors.Count;

                if (unsuccessfulInserts == modelList.Count)
                {
                    _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(CreateMany), DateTime.UtcNow, orderList, ex);
                    ResultObject<Error> noInsertsResponse = OrderError.Create;
                    return StatusCode(500, noInsertsResponse);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(CreateMany), DateTime.UtcNow, orderList, ex);
                return StatusCode(500, OrderError.Create);
            }

            var responseList = new List<OrderResponse>();
            foreach(var orderModel in modelList)
            {
                var responseModel = new OrderResponse(orderModel);
                responseList.Add(responseModel);
            }

            ResultObject<List<OrderResponse>> response = responseList;

            if (unsuccessfulInserts > 0)
                return StatusCode(207, response);

            return Ok(response);
        }

        [HttpPut("{orderId}/User/me")]
        [Authorize]
        public async Task<IActionResult> UpdateOneFromMe(OrderRequestDTO order, string orderId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(UpdateOneFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userError = UserError.GetByEmail;
                return StatusCode(500, userError);
            }

            if (user is null) return Forbid();

            if (order.Items.Count == 0)
            {
                ResultObject<Error> itemsResponse = OrderError.InvalidItems;
                return BadRequest(itemsResponse);
            }

            var orderModel = await _ordersContext.GetOneFromUser(user.Id, orderId);

            if (orderModel is null) return NotFound();

            order.UserId = user.Id;

            var model = order.ToModel();

            if (model is null)
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            model.UpdateFromModel(orderModel);

            try
            {
                var amountUpdateChanges = await _ordersContext.Update(model, orderId, user.Id);

                if (amountUpdateChanges == 0)
                {
                    ResultObject<Error> invalidUpdate = OrderError.InvalidUpdate;
                    return BadRequest(invalidUpdate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(UpdateOneFromMe), DateTime.UtcNow, new { order, orderId }, ex);
                ResultObject<Error> updateError = OrderError.Update;
                return StatusCode(500, updateError);
            }

            return NoContent();
        }

        [HttpPut("{orderId}/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateOne(OrderRequestDTO order, string orderId, int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (order.Items.Count == 0)
            {
                ResultObject<Error> itemsResponse = OrderError.InvalidItems;
                return BadRequest(itemsResponse);
            }

            order.UserId = userId;

            var model = order.ToModel();

            if (model is null)
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            var orderModel = await _ordersContext.GetOneFromUser(userId, orderId);

            if (orderModel is null) return NotFound();

            model.UpdateFromModel(orderModel);

            try
            {
                var amountUpdateChanges = await _ordersContext.Update(model, orderId, userId);

                if (amountUpdateChanges == 0)
                {
                    ResultObject<Error> invalidUpdate = OrderError.InvalidUpdate;
                    return BadRequest(invalidUpdate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(UpdateOne), DateTime.UtcNow, new { order, orderId, userId}, ex);
                ResultObject<Error> updateError = OrderError.Update;
                return StatusCode(500, updateError);
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

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(UpdateManyFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userError = UserError.GetByEmail;
                return StatusCode(500, userError);
            }

            if (user is null) return Forbid();

            var validModel = orderUpdateList.Any(o => o.Order is not null && o.Order.Items.Count > 0 && !string.IsNullOrEmpty(o.OrderId));

            if (!validModel)
            {
                ResultObject<Error> itemsResponse = OrderError.InvalidItems;
                return UnprocessableEntity(itemsResponse);
            }

            List<Order> modelList = new();

            foreach (var order in orderUpdateList)
            {
                var originalOrder = await _ordersContext.GetOneFromUser(user.Id, order.OrderId!);

                if (originalOrder is null)
                {
                    ResultObject<Error> notFoundBulkRes = OrderError.NotFoundBulk;
                    return UnprocessableEntity(notFoundBulkRes);
                }

                var orderModel = order.Order!.ToModel();

                if (orderModel is null)
                {
                    ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                    return UnprocessableEntity(parsingResponse);
                }

                orderModel.Id = order.OrderId!;

                modelList.Add(orderModel);
            }

            if (modelList.Count == 0)
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            long modificationsTotal = 0;
            try
            {
                modificationsTotal = await _ordersContext.UpdateMany(modelList, user.Id);

                if (modificationsTotal == 0)
                {
                    ResultObject<Error> invalidResponse = OrderError.InvalidUpdate;
                    return BadRequest(invalidResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(UpdateManyFromMe), DateTime.UtcNow, orderUpdateList, ex);
                ResultObject<Error> updateError = OrderError.Update;
                return StatusCode(500, updateError);
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
                return UnprocessableEntity(OrderError.InvalidItems);

            List<Order> modelList = new();

            foreach(var order in orderUpdateList)
            {
                var originalOrder = await _ordersContext.GetOneFromUser(userId, order.OrderId!);

                if (originalOrder is null)
                {
                    ResultObject<Error> notFoundBulkRes = OrderError.NotFoundBulk;
                    return UnprocessableEntity(notFoundBulkRes);
                }

                var orderModel = order.Order!.ToModel();

                if (orderModel is null)
                {
                    ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                    return UnprocessableEntity(parsingResponse);
                }

                orderModel.Id = order.OrderId!;

                modelList.Add(orderModel);
            }

            if (modelList.Count == 0)
            {
                ResultObject<Error> parsingResponse = OrderError.InvalidParsing;
                return StatusCode(500, parsingResponse);
            }

            long modificationsTotal = 0;
            try
            {
                modificationsTotal = await _ordersContext.UpdateMany(modelList, userId);

                if (modificationsTotal == 0)
                {
                    ResultObject<Error> invalidResponse = OrderError.InvalidUpdate;
                    return BadRequest(invalidResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(UpdateMany), DateTime.UtcNow, new { orderUpdateList, userId }, ex);
                ResultObject<Error> updateError = OrderError.Update;
                return StatusCode(500, updateError);
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

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(DeleteOneFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userError = UserError.GetByEmail;
                return StatusCode(500, userError);
            }

            if (user is null) return Forbid();

            bool wasDeleted = false;
            try
            {
                wasDeleted = await _ordersContext.Delete(orderId, user.Id);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(DeleteOneFromMe), DateTime.UtcNow, orderId, ex);
                ResultObject<Error> deleteResponse = OrderError.Delete;
                return StatusCode(500, deleteResponse);
            }

            if (!wasDeleted)
            {
                ResultObject<Error> invalidResponse = OrderError.InvalidDelete;
                return BadRequest(invalidResponse);
            }

            return NoContent();
        }

        [HttpDelete("{orderId}/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOne(string orderId, int userId)
        {
            if (userId == 0)
            {
                ResultObject<Error> invalidIdResponse = UserError.InvalidId;
                return BadRequest(invalidIdResponse);
            }

            bool wasDeleted = false;
            try
            {
                wasDeleted = await _ordersContext.Delete(orderId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(DeleteOne), DateTime.UtcNow, new { orderId, userId }, ex);
                ResultObject<Error> deleteError = OrderError.Delete;
                return StatusCode(500, deleteError);
            }

            if (!wasDeleted)
            {
                ResultObject<Error> invalidError = OrderError.InvalidDelete;
                return BadRequest(invalidError);
            }

            return NoContent();
        }

        [HttpDelete("many/User/me")]
        [Authorize]
        public async Task<IActionResult> DeleteManyFromMe(List<string> orderId)
        {
            var authUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(authUserEmail)) return Forbid();

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(DeleteManyFromMe), DateTime.UtcNow, authUserEmail, ex);
                ResultObject<Error> userError = UserError.GetByEmail;
                return StatusCode(500, userError);
            }

            if (user is null) return Forbid();

            var validIds = !orderId.Any(string.IsNullOrEmpty);

            if (!validIds)
            {
                ResultObject<Error> invalidOrderIdResponse = OrderError.InvalidIds;
                return BadRequest(invalidOrderIdResponse);
            }

            var amountToDelete = orderId.Count;
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.In(o => o.Id, orderId),
                Builders<Order>.Filter.Eq(o => o.UserId, user.Id)
                );

            bool wereDeleted = false;
            try
            {
                wereDeleted = await _ordersContext.DeleteMany(orderId, amountToDelete);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(DeleteManyFromMe), DateTime.UtcNow, orderId, ex);
                ResultObject<Error> deleteError = OrderError.Delete;
                return StatusCode(500, deleteError);
            }

            if (!wereDeleted)
            {
                ResultObject<Error> invalidResponse = OrderError.InvalidDelete;
                return BadRequest(invalidResponse);
            }

            return NoContent();
        }

        [HttpDelete("many/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMany(List<string> orderId, int userId)
        {
            var validIds = orderId.Any(id => id is not null);

            if (!validIds)
            {
                ResultObject<Error> invalidOrderIdResponse = OrderError.InvalidIds;
                return BadRequest(invalidOrderIdResponse);
            }

            var amountToDelete = orderId.Count;
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.In(o => o.Id, orderId),
                Builders<Order>.Filter.Eq(o => o.UserId, userId)
                );

            bool wereDeleted = false;
            try
            {
                wereDeleted = await _ordersContext.DeleteMany(orderId, amountToDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in {Controller} with method {Method} at {Time} with properties {@Props} because {@Exception}",
                           nameof(OrderController), nameof(DeleteMany), DateTime.UtcNow, new { orderId, userId }, ex);
                ResultObject<Error> deleteError = OrderError.Delete;
                return StatusCode(500, deleteError);
            }

            if (!wereDeleted)
            {
                ResultObject<Error> invalidResponse = OrderError.InvalidDelete;
                return BadRequest(invalidResponse);
            }

            return NoContent();
        }
    }
}
