﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Pizzeria.Database;
using Pizzeria.Database.Models;
using Pizzeria.DTOs.Orders;
using Pizzeria.DTOs.Users;
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

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, UserError.GetByEmail);
            }

            if(user is null) return Forbid();

            List<Order>? orders;
            try
            {
                orders = await _ordersContext.GetAllFromUser(user.Id);
            }
            catch(Exception ex)
            {
                return StatusCode(500, OrderError.GetAllFromUser);
            }

            if (orders is null
                || orders.Count == 0)
                return NotFound();

            var response = orders.Select(o => o.ToDTO()).ToList();
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
                return StatusCode(500, OrderError.GetAllFromUser);
            }

            if(orders is null
                || orders.Count == 0)
                return NotFound();

            var response = orders.Select(o => o.ToDTO()).ToList();
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
                return StatusCode(500, UserError.GetByEmail);
            }

            if (user is null) return Forbid();

            Order? order;
            try
            {
                order = await _ordersContext.GetOneFromUser(user.Id, orderId);
            }
            catch(Exception ex)
            {
                return StatusCode(500, OrderError.GetOneFromUser);
            }

            if (order is null) return NotFound();

            var response = order.ToDTO();
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
                return StatusCode(500, OrderError.GetOneFromUser);
            }

            if(order is null) return NotFound();

            var response = order.ToDTO();
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
                return StatusCode(500, UserError.GetByEmail);
            }

            if (user is null) return Forbid();

            if (order.Items.Count == 0) return BadRequest(OrderError.InvalidItems);

            var model = order.ToModel();

            if (model is null
                || (model.Items is null || model.Items.Count == 0))
                return StatusCode(500, OrderError.InvalidParsing);

            model.UserId = user.Id;

            try
            {
                await _ordersContext.Create(model);
            }
            catch (Exception)
            {
                return StatusCode(500, OrderError.Create);
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

            if (order.Items.Count == 0) return BadRequest(OrderError.InvalidItems);

            var model = order.ToModel();

            if (model is null)
                return StatusCode(500, OrderError.InvalidParsing);

            try
            {
                await _ordersContext.Create(model);
            }
            catch(Exception)
            {
                return StatusCode(500, OrderError.Create);
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

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, UserError.GetByEmail);
            }

            if (user is null) return Forbid();

            var validOrderItems = orderList.Sum(o => o.Items.Count) > 0;

            if (!validOrderItems) return BadRequest(OrderError.Create);

            var modelList = orderList.Select(o => o.ToModel()).ToList();
            modelList.ForEach(o => o.UserId = user.Id);

            var validModelItems = modelList.Any(o => o.Items is not null || o.Items!.Count == 0);
            if (modelList is null
                || modelList.Count == 0
                || !validModelItems) return StatusCode(500, OrderError.InvalidParsing);

            int unsuccessfulInserts = 0;
            try
            {
                await _ordersContext.CreateMany(modelList);
            }
            catch (MongoBulkWriteException<Order> ex)
            {
                unsuccessfulInserts = ex.WriteErrors.Count;

                if (unsuccessfulInserts == modelList.Count)
                    return StatusCode(500, OrderError.Create);
            }
            catch (Exception)
            {
                return StatusCode(500, OrderError.Create);
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

            if (!validOrderItems) return BadRequest(OrderError.InvalidItems);

            var modelList = orderList.Select(o => o.ToModel()).ToList();

            var validModelItems = modelList.Any(o => o.Items is not null || o.Items!.Count == 0);
            if (modelList is null
                || modelList.Count == 0
                || !validModelItems) return StatusCode(500, OrderError.InvalidParsing);

            int unsuccessfulInserts = 0;
            try
            {
                await _ordersContext.CreateMany(modelList);
            }
            catch(MongoBulkWriteException<Order> ex)
            {
                unsuccessfulInserts = ex.WriteErrors.Count;

                if(unsuccessfulInserts == modelList.Count)
                    return StatusCode(500, OrderError.Create);
            }
            catch(Exception)
            {
                return StatusCode(500, OrderError.Create);
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

            User? user;
            try
            {
                user = await _userService.GetByEmail(authUserEmail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, UserError.GetByEmail);
            }

            if (user is null) return Forbid();

            if (order.Items.Count == 0) return BadRequest(OrderError.InvalidItems);

            var orderModel = await _ordersContext.GetOneFromUser(user.Id, orderId);

            if (orderModel is null) return NotFound();

            order.UserId = user.Id;

            var model = order.ToModel();

            if (model is null)
                return StatusCode(500, OrderError.InvalidParsing);

            model.UpdateFromModel(orderModel);

            try
            {
                var amountUpdateChanges = await _ordersContext.Update(model, orderId, user.Id);

                if (amountUpdateChanges == 0) return BadRequest(OrderError.InvalidUpdate);
            }
            catch (Exception)
            {
                return StatusCode(500, OrderError.Update);
            }

            return NoContent();
        }

        [HttpPut("{orderId}/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateOne(OrderRequestDTO order, string orderId, int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (order.Items.Count == 0) return BadRequest(OrderError.InvalidItems);

            order.UserId = userId;

            var model = order.ToModel();

            if (model is null)
                return StatusCode(500, OrderError.InvalidParsing);

            var orderModel = await _ordersContext.GetOneFromUser(userId, orderId);

            if (orderModel is null) return NotFound();

            model.UpdateFromModel(orderModel);

            try
            {
                var amountUpdateChanges = await _ordersContext.Update(model, orderId, userId);

                if (amountUpdateChanges == 0) return BadRequest(OrderError.InvalidUpdate);
            }
            catch(Exception)
            {
                return StatusCode(500, OrderError.Update);
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
                return StatusCode(500, UserError.GetByEmail);
            }

            if (user is null) return Forbid();

            var validModel = orderUpdateList.Any(o => o.Order is not null && o.Order.Items.Count > 0 && !string.IsNullOrEmpty(o.OrderId));

            if (!validModel)
                return UnprocessableEntity(OrderError.InvalidItems);

            List<Order> modelList = new();

            foreach (var order in orderUpdateList)
            {
                var originalOrder = await _ordersContext.GetOneFromUser(user.Id, order.OrderId!);

                if (originalOrder is null)
                    return UnprocessableEntity(OrderError.NotFoundBulk);

                var orderModel = order.Order!.ToModel();

                if (orderModel is null) return UnprocessableEntity(OrderError.InvalidParsing);

                orderModel.Id = order.OrderId!;

                modelList.Add(orderModel);
            }

            if (modelList.Count == 0) return StatusCode(500, OrderError.InvalidParsing);

            var updateTasks = new List<Task<UpdateResult>>();

            long modificationsTotal = 0;
            try
            {
                modificationsTotal = await _ordersContext.UpdateMany(modelList, user.Id);

                if (modificationsTotal == 0) return BadRequest(OrderError.InvalidUpdate);
            }
            catch (Exception)
            {
                return StatusCode(500, OrderError.Update);
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
                    return UnprocessableEntity(OrderError.GetOneFromUser);

                var orderModel = order.Order!.ToModel();

                if (orderModel is null) return UnprocessableEntity(OrderError.InvalidParsing);

                orderModel.Id = order.OrderId!;

                modelList.Add(orderModel);
            }

            if(modelList.Count == 0) return StatusCode(500, OrderError.InvalidParsing);

            var updateTasks = new List<Task<UpdateResult>>();

            long modificationsTotal = 0;
            try
            {
                modificationsTotal = await _ordersContext.UpdateMany(modelList, userId);

                if (modificationsTotal == 0) return BadRequest(OrderError.InvalidUpdate);
            }
            catch (Exception)
            {
                return StatusCode(500, OrderError.Update);
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
                return StatusCode(500, UserError.GetByEmail);
            }

            if (user is null) return Forbid();

            bool wasDeleted = false;
            try
            {
                wasDeleted = await _ordersContext.Delete(orderId, user.Id);
            }
            catch(Exception ex)
            {
                return StatusCode(500, OrderError.Delete);
            }

            if (!wasDeleted) return BadRequest(OrderError.InvalidDelete);

            return NoContent();
        }

        [HttpDelete("{orderId}/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOne(string orderId, int userId)
        {
            if (userId == 0) return BadRequest(UserError.InvalidId);

            bool wasDeleted = false;
            try
            {
                wasDeleted = await _ordersContext.Delete(orderId, userId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, OrderError.Delete);
            }

            if (!wasDeleted) return BadRequest(OrderError.InvalidDelete);

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
                return StatusCode(500, UserError.GetByEmail);
            }

            if (user is null) return Forbid();

            var validIds = !orderId.Any(string.IsNullOrEmpty);

            if (!validIds) return BadRequest(OrderError.InvalidIds);

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
                return StatusCode(500, OrderError.Delete);
            }

            if (!wereDeleted) return BadRequest(OrderError.InvalidDelete);

            return NoContent();
        }

        [HttpDelete("many/User/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMany(List<string> orderId, int userId)
        {
            var validIds = orderId.Any(id => id is not null);

            if (!validIds) return BadRequest(OrderError.InvalidIds);

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
                return StatusCode(500, OrderError.Delete);
            }

            if (!wereDeleted) return BadRequest(OrderError.InvalidDelete);

            return NoContent();
        }
    }
}
