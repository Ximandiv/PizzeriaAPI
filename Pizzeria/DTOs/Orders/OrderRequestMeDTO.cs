using Pizzeria.Database.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pizzeria.DTOs.Orders
{
    public class OrderRequestMeDTO
    {
        [Required]
        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();

        [DataType(DataType.DateTime)]
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Order ToModel()
            => new Order()
            {
                Items = Items.Select(x => x.ToModel()).ToList()
            };
    }
}
