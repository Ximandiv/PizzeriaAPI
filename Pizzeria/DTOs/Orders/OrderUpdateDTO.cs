using System.ComponentModel.DataAnnotations;

namespace Pizzeria.DTOs.Orders
{
    public class OrderUpdateDTO
    {
        [Required]
        public string? OrderId { get; set; }

        [Required]
        public OrderRequestDTO? Order { get; set; }
    }
}
