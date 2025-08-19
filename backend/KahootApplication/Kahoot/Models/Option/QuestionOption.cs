namespace Kahoot.Models.Option
{
    public class QuestionOption
    {
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }
}