﻿using Pizzeria.Database.Models;

namespace Pizzeria.DTOs.Orders
{
    public class OrderResponse
    {
        public string OrderId { get; set; }

        public int UserId { get; set; }

        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public OrderResponse(Order orderModel)
        {
            OrderId = orderModel.Id!;
            UserId = orderModel.UserId!;
            Items = orderModel.Items.Select(i => i.ToDTO()).ToList();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
