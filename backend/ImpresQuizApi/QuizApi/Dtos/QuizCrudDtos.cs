namespace QuizApi.Dtos
{
    public record CreateQuizRequest(string Title, string? Description, int CreatedBy, bool IsPublished = false);
    public record CreateQuizResponse(int QuizId);

    public record UpdateQuizRequest(string Title, string? Description);
    public record PublishQuizRequest(bool IsPublished);
}