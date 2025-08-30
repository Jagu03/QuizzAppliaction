namespace QuizApi.Dtos
{
    public record JoinPlayerRequest(string PinCode, string DisplayName);
    public record JoinPlayerResponse(int PlayerId, Guid GameSessionId);
}
