namespace Kahoot.Models.Assignment
{
    public class PublishAssignmentDto
    {
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? TimeLimitSeconds { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleOptions { get; set; }
        public int MaxAttempts { get; set; }
        public int CreatedByStaffId { get; set; }
    }
}
