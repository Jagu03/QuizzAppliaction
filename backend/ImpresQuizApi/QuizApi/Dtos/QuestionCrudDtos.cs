namespace QuizApi.Dtos
{
    public record CreateQuestionRequest(
        string Text, byte QuestionType = 0, int TimeLimitSec = 20, int Points = 1000, string? MediaUrl = null, int? OrderNo = null);
    public record CreateQuestionResponse(int QuestionId);

    public record UpdateQuestionRequest(
        string Text, byte QuestionType, int TimeLimitSec, int Points, string? MediaUrl);
    public record ReorderQuestionRequest(int NewOrderNo);
}