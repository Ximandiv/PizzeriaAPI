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

        public static implicit operator ResultObject<T>(GenericResponse message) => new ResultObject<T>(true, (T)(object)message);
        public static implicit operator ResultObject<T>(List<GenericResponse> message) => new ResultObject<T>(true, (T)(object)message);
        public static implicit operator ResultObject<T>(string message) => new ResultObject<T>(true, (T)(object)message);

        public static implicit operator ResultObject<T>(Error message) => new ResultObject<T>(false, (T)(object)message);
    }
}
