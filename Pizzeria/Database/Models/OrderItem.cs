using Pizzeria.DTOs.Orders;
using System.ComponentModel.DataAnnotations;

namespace Pizzeria.Database.Models
{
    public class OrderItem
    {
        [StringLength(50)]
        public required string Name { get; set; }

        [Required]
        [Range(1, 9999.99)]
        public decimal Price { get; set; } = 0;

        public OrderItemDTO ToDTO()
            => new OrderItemDTO()
            {
                Name = Name,
                Price = Price
            };
    }
}
