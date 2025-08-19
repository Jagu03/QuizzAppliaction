namespace Kahoot.Models.Question
{
    public class CreateQuestionRequest
    {
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectOptionNumber { get; set; } // 1-based index
    }
}