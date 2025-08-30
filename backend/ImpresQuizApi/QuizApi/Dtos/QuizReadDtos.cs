// Dtos/QuizReadDtos.cs
namespace QuizApi.Dtos
{
    public record QuizDetailDto(
        int QuizId, string Title, string? Description, bool IsPublished, int CreatedBy,
        DateTime CreatedAt, DateTime? UpdatedAt, int QuestionCount);
}
