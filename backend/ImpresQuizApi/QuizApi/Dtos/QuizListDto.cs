
namespace QuizApi.Dtos
{
    public record QuizListDto(int QuizId, string Title, bool IsPublished, int CreatedBy, DateTime CreatedAt, DateTime? UpdatedAt);
}
