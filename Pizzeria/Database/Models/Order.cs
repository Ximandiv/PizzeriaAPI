using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pizzeria.DTOs.Orders;
using System.ComponentModel.DataAnnotations;

namespace Pizzeria.Database.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        [DataType(DataType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public OrderRequestDTO ToDTO()
            => new OrderRequestDTO()
            {
                UserId = UserId,
                Items = Items.Select(i => i.ToDTO()).ToList(),
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
            };

        public void UpdateFromModel(Order order)
        {
            Id = order.Id;
            UserId = order.UserId;
            CreatedAt = order.CreatedAt;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
