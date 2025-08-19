namespace Kahoot.Models.Group
{
    public class CreateClassGroupDto
    {
        public int ClassId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int CreatedByStaffId { get; set; }
    }
}
