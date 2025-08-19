namespace Kahoot.Models.Auth
{
    public class LoginRequest
    {
        public string? LoginCode { get; set; }
        public string? UserPassword { get; set; }
        public string? ModuleName { get; set; }
        public string? IPAddr { get; set; }
        public string? SessId { get; set; }
        public string? Browser { get; set; }
        public string? BVersion { get; set; }
        public string? PCUSName { get; set; }
    }
}
