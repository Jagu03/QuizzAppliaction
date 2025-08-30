using Dapper;
using QuizApi.Dtos;
using QuizApi.Infrastructure;
using QuizApi.Repositories;

namespace QuizApi.Implementations
{
    public class PlayerRepository : RepositoryBase, IPlayerRepository
    {
        public PlayerRepository(IDbConnectionFactory f) : base(f) { }

        public async Task<JoinPlayerResponse> JoinByPinAsync(string pinCode, string displayName)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@PinCode", pinCode);
            p.Add("@DisplayName", displayName);
            p.Add("@PlayerId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            p.Add("@GameSessionId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

            await conn.ExecuteAsync("dbo.sp_JoinPlayerByPin", p, commandType: System.Data.CommandType.StoredProcedure);
            return new JoinPlayerResponse(p.Get<int>("@PlayerId"), p.Get<Guid>("@GameSessionId"));
        }
    }
}
