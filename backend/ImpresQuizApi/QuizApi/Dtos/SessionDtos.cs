namespace QuizApi.Dtos
{
    public record SessionStatusDto(
        Guid GameSessionId,
        int QuizId,
        int HostUserId,
        string PinCode,
        byte Status,
        DateTime? StartedAt,
        DateTime? EndedAt,
        DateTime CreatedAt
    );

    public record PlayerRow(int PlayerId, string DisplayName, bool IsKicked, DateTime JoinedAt);

    public record QuestionStatRow(Guid GameSessionId, int QuestionId, int OrderNo, int TotalSubmissions, int CorrectSubmissions, double AvgTimeTakenMs);

    public record PlayerAnswerRow(
        long PlayerAnswerId,
        Guid GameSessionId,
        int PlayerId,
        int QuestionId,
        int? ChoiceId,
        bool IsCorrect,
        int? TimeTakenMs,
        int ScoreAwarded,
        DateTime SubmittedAt
    );
}