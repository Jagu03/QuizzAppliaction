using Dapper;
using QuizApi.Dtos;
using QuizApi.Infrastructure;
using QuizApi.Repositories;

namespace QuizApi.Implementations
{
    public class GameSessionRepository : RepositoryBase, IGameSessionRepository
    {
        public GameSessionRepository(IDbConnectionFactory f) : base(f) { }

        public async Task<CreateGameSessionResponse> CreateAsync(int quizId, int hostUserId)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@QuizId", quizId);
            p.Add("@HostUserId", hostUserId);
            p.Add("@GameSessionId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);
            p.Add("@PinCode", dbType: System.Data.DbType.String, size: 6, direction: System.Data.ParameterDirection.Output);

            await conn.ExecuteAsync("dbo.sp_CreateGameSession", p, commandType: System.Data.CommandType.StoredProcedure);
            var id = p.Get<Guid>("@GameSessionId");
            var pin = p.Get<string>("@PinCode")!;
            return new CreateGameSessionResponse(id, pin);
        }

        public async Task StartAsync(Guid gameSessionId)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@GameSessionId", gameSessionId);
            await conn.ExecuteAsync("dbo.sp_StartGame", p, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task EndAsync(Guid gameSessionId)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@GameSessionId", gameSessionId);
            await conn.ExecuteAsync("dbo.sp_EndGame", p, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task CancelAsync(Guid gameSessionId)
        {
            using var conn = Open();
            await conn.ExecuteAsync("dbo.sp_CancelGame",
                new { GameSessionId = gameSessionId },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
