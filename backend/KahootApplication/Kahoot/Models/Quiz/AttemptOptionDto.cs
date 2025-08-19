namespace Kahoot.Models.Quiz
{
    public class AttemptOptionDto
    {
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }
}
