namespace Kahoot.Models.Quiz
{
    public class AttemptQuestionDto
    {
        public int AttemptAnswerId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }
}
