namespace QuizApi.Dtos
{
    public record CreateGameSessionRequest(int QuizId, int HostUserId);
    public record CreateGameSessionResponse(Guid GameSessionId, string PinCode);

    public record StartEndRequest(Guid GameSessionId);
}
