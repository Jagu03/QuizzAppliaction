namespace QuizApi.Dtos
{
    public record SubmitAnswerRequest(Guid GameSessionId, int PlayerId, int QuestionId, int ChoiceId, int TimeTakenMs);
    public record SubmitAnswerResponse(bool Accepted);
}
