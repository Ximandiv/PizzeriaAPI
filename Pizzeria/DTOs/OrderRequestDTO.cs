using Pizzeria.Database.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pizzeria.DTOs
{
    public class OrderRequestDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

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
                UserId = UserId,
                Items = Items.Select(x => x.ToModel()).ToList()
            };
    }
}
