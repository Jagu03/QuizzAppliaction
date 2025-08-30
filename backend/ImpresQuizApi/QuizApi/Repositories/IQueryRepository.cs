using QuizApi.Dtos;

namespace QuizApi.Repositories
{
    public interface IQueryRepository
    {
        Task<IEnumerable<QuestionDto>> GetQuestionsWithChoicesAsync(int quizId);
        Task<SessionStatusDto?> GetSessionAsync(Guid sessionId);
        Task<IEnumerable<PlayerRow>> GetPlayersAsync(Guid sessionId);
        Task<IEnumerable<QuestionStatRow>> GetQuestionStatsAsync(Guid sessionId);
        Task<IEnumerable<PlayerAnswerRow>> GetPlayerAnswersAsync(Guid sessionId, int playerId);
        Task<bool> KickPlayerAsync(int playerId, Guid sessionId);
        Task<SessionStatusDto?> GetActiveSessionByPinAsync(string pinCode);
        Task<bool> UnkickPlayerAsync(int playerId, Guid sessionId);
        Task<IEnumerable<QuizListDto>> GetQuizzesAsync(int? createdBy);
        Task<IEnumerable<QuestionAnswerDto>> GetAnswersByQuestionAsync(Guid sessionId, int questionId);
        Task<IEnumerable<ChoiceSummaryDto>> GetChoiceSummaryAsync(Guid sessionId, int questionId);
        Task<IEnumerable<PlayerRow>> GetActivePlayersAsync(Guid sessionId);
    }
}
