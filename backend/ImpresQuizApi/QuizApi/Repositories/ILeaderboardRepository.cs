using QuizApi.Dtos;

namespace QuizApi.Repositories
{
    public interface ILeaderboardRepository
    {
        Task<IEnumerable<LeaderboardRow>> GetAsync(Guid gameSessionId);
    }
}
