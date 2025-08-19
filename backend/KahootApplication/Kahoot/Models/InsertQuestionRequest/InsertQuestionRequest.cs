namespace Kahoot.Models.InsertQuestionRequest
{
    public class InsertQuestionRequest
    {
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Option4 { get; set; } = string.Empty;
        public int CorrectOptionNumber { get; set; } // 1..4
    }

    public class UpdateQuestionRequest
    {
        public int QuestionId { get; set; }
        public int CategoryId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Option4 { get; set; } = string.Empty;
        public int CorrectOptionNumber { get; set; } // 1..4
    }
}
