namespace Kahoot.Helpers
{
    public class JwtSettings
    {
        public string? Secret { get; set; }
        public int ExpireHours { get; set; }
    }
}
