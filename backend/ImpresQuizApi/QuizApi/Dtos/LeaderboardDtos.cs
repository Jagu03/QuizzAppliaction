namespace QuizApi.Dtos
{
    public record LeaderboardRow(
        int PlayerId,
        string DisplayName,
        int TotalScore,
        int CorrectCount,
        int AnsweredCount,
        DateTime JoinedAt
    );
}
