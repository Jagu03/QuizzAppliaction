using Kahoot.Models.Option;

namespace Kahoot.Models.Question
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int? CorrectOptionId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<QuestionOption> Options { get; set; } = new();
    }
}
