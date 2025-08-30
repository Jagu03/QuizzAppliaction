using QuizApi.Dtos;

namespace QuizApi.Repositories
{
    public interface IGameSessionRepository
    {
        Task<CreateGameSessionResponse> CreateAsync(int quizId, int hostUserId);
        Task StartAsync(Guid gameSessionId);
        Task EndAsync(Guid gameSessionId);
        Task CancelAsync(Guid gameSessionId);
    }
   
}
