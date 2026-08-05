namespace Excel.Service
{
    public class LoginResult
    {
        public bool Success { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; } = "";

        public string? Username { get; set; }

        public string? Token { get; set; }
    }
}