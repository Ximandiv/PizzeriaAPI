using Pizzeria.Database.Models;
using System.ComponentModel.DataAnnotations;

namespace Pizzeria.DTOs.Orders
{
    public class OrderItemDTO
    {
        [StringLength(50)]
        public required string Name { get; set; }

        [Required]
        [Range(1, 9999.99)]
        public decimal Price { get; set; } = 0;

        public OrderItem ToModel()
            => new OrderItem()
            {
                Name = Name,
                Price = Price
            };
    }
}
