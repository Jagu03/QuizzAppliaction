namespace Kahoot.Models.Auth
{
    public class UserInfoResponse
    {
        public int UserId { get; set; }
        public string? LoginCode { get; set; }
        public string? WelcomeName { get; set; }
        public string? LastLogin { get; set; }
        public long LastLoginId { get; set; }
        public int? ModId { get; set; }
        public string? SessionId { get; set; }
        public string? Image { get; set; }
        public int StaffId { get; set; }
        public string? ImagePath { get; set; }
        public string? Designation { get; set; }
        public string? CharitiesName { get; set; }
        public string? CharitiesLocation { get; set; }
        public int CoeVer { get; set; }
    }
}
