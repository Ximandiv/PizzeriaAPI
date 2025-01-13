using Pizzeria.DTOs.Orders;
using Pizzeria.DTOs.Users;

namespace Pizzeria.DTOs
{
    public class ResultObject<T>
    {
        public bool WasSuccessful { get; set; }
        public T Message { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        private ResultObject(bool wasSuccessful, T message)
        {
            WasSuccessful = wasSuccessful;
            Message = message;
        }
        private ResultObject()
        {
            
        }

        public static implicit operator ResultObject<T>(UserResponse message) => new ResultObject<T>(true, (T)(object)message);
        public static implicit operator ResultObject<T>(List<UserResponse> message) => new ResultObject<T>(true, (T)(object)message);

        public static implicit operator ResultObject<T>(OrderResponse message) => new ResultObject<T>(true, (T)(object)message);
        public static implicit operator ResultObject<T>(List<OrderResponse> message) => new ResultObject<T>(true, (T)(object)message);

        public static implicit operator ResultObject<T>(string message) => new ResultObject<T>(true, (T)(object)message);

        public static implicit operator ResultObject<T>(Error message) => new ResultObject<T>(false, (T)(object)message);
    }
}
