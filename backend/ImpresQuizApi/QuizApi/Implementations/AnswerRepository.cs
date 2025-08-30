using Dapper;
using Microsoft.Data.SqlClient;
using QuizApi.Dtos;
using QuizApi.Infrastructure;
using QuizApi.Repositories;

namespace QuizApi.Implementations
{
    public class AnswerRepository : RepositoryBase, IAnswerRepository
    {
        public AnswerRepository(IDbConnectionFactory f) : base(f) { }

        public async Task<bool> SubmitAsync(SubmitAnswerRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@GameSessionId", req.GameSessionId);
            p.Add("@PlayerId", req.PlayerId);
            p.Add("@QuestionId", req.QuestionId);
            p.Add("@ChoiceId", req.ChoiceId);
            p.Add("@TimeTakenMs", req.TimeTakenMs);

            try
            {
                await conn.ExecuteAsync("dbo.sp_SubmitAnswer", p, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            }
            catch (SqlException ex) when (ex.Number == 50000) // RAISERROR default user errors
            {
                // You raised friendly messages inside proc. You can inspect ex.Message if needed.
                return false;
            }
        }
    }
}
