namespace Demo1.DTOs
{
    public class UserQuestionLogDTO
    {
        public int Id { get; set; }

        public string? Question { get; set; }

        public string? MatchedAnswer { get; set; }

        public DateTime? AskedAt { get; set; }

        public int? Score { get; set; }
    }
    public class OffsetPagingResponseDto<T>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public List<T> Data { get; set; } = new();
    }
    public class KeysetPagingResponseDto<T>
    {
        public int PageSize { get; set; }

        public int? NextLastId { get; set; }

        public bool HasMore { get; set; }

        public List<T> Data { get; set; } = new();
    }
}
