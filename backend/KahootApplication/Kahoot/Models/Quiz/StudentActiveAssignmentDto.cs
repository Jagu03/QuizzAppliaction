namespace Kahoot.Models.Quiz
{
    public class StudentActiveAssignmentDto
    {
        public int StudentAssignmentId { get; set; }
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? TimeLimitSeconds { get; set; }
        public int MaxAttempts { get; set; }
        public int AttemptCount { get; set; }
        public int CategoryId { get; set; }
    }
}
