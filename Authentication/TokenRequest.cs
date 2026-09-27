namespace Demo1.Authentication
{
    public class TokenRequest
    {
        public string ClientId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Role { get; set; } = "User";
    }
}
