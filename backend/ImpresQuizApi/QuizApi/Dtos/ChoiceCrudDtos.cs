namespace QuizApi.Dtos
{
    public record CreateChoiceRequest(string Text, bool IsCorrect = false, int? OrderNo = null);
    public record CreateChoiceResponse(int ChoiceId);

    public record UpdateChoiceRequest(string Text, bool IsCorrect, int OrderNo);
    public record ReorderChoiceRequest(int NewOrderNo);
}