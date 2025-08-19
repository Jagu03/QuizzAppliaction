using Kahoot.Models.Assignment;

namespace Kahoot.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<int> PublishAsync(PublishAssignmentDto dto);
        Task<(int assignmentId, int groupId)> PublishForClassAsync(PublishForClassDto dto);
    }

    public class PublishForClassDto
    {
        public int ClassId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? TimeLimitSeconds { get; set; }
        public bool ShuffleQuestions { get; set; } = true;
        public bool ShuffleOptions { get; set; } = true;
        public int MaxAttempts { get; set; } = 1;
        public int CreatedByStaffId { get; set; }
    }
}
