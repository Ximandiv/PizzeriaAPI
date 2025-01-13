using Pizzeria.DTOs.Orders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pizzeria.Database.Models
{
    [NotMapped]
    public class OrderUpdate
    {
        public Order Order { get; set; }

        public OrderUpdate(string orderId, OrderRequestDTO orderDTO)
        {
            Order = orderDTO.ToModel();
            Order.Id = orderId;
        }
    }
}
