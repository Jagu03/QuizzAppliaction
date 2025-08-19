namespace Kahoot.Models.Auth
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? WelcomeName { get; set; }
        public string? LastLogin { get; set; }
        public int UserId { get; set; }
        public string? Image { get; set; }
        public int Result { get; set; }
        public string? Message { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
        public List<string> DeniedModules { get; set; } = new List<string>();
    }
}
