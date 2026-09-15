namespace Demo1.DTO
{
    public class SampleDemo
    {
        public class SampleDemoRequest
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class SampleDemoResponse
        {
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
