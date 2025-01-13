namespace TestProject1.Responses
{
    internal class TestResponseObject<T>
    {
        public bool WasSuccessful { get; set; }
        public T? Message { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        public TestResponseObject()
        {
            
        }
    }
}
