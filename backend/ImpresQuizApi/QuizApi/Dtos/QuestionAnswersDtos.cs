// Dtos/QuestionAnswersDtos.cs
namespace QuizApi.Dtos
{
    public record QuestionAnswerDto(
        long PlayerAnswerId, Guid GameSessionId, int PlayerId, string PlayerName,
        int QuestionId, int? ChoiceId, string? ChoiceText, bool IsCorrect,
        int? TimeTakenMs, int ScoreAwarded, DateTime SubmittedAt);

    public record ChoiceSummaryDto(int ChoiceId, string Text, int Count, double Percent);
}
