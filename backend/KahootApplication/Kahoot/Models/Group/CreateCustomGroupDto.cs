namespace Kahoot.Models.Group
{
    public class CreateCustomGroupDto
    {
        public string GroupName { get; set; } = string.Empty;
        public int CreatedByStaffId { get; set; }
        public string StudentIdsCsv { get; set; } = string.Empty; // "12,45,78"
    }
}
