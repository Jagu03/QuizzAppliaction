using Dapper;
using QuizApi.Dtos;
using QuizApi.Infrastructure;
using QuizApi.Repositories;

namespace QuizApi.Implementations
{
    public class LeaderboardRepository : RepositoryBase, ILeaderboardRepository
    {
        public LeaderboardRepository(IDbConnectionFactory f) : base(f) { }

        public async Task<IEnumerable<LeaderboardRow>> GetAsync(Guid gameSessionId)
        {
            using var conn = Open();
            var p = new { GameSessionId = gameSessionId };
            return await conn.QueryAsync<LeaderboardRow>(
                "dbo.sp_GetLeaderboard", p, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
