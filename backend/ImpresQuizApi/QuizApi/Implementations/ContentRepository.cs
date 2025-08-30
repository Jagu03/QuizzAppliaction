using Dapper;
using QuizApi.Dtos;
using QuizApi.Infrastructure;
using QuizApi.Repositories;

namespace QuizApi.Implementations
{
    public class ContentRepository : RepositoryBase, IContentRepository
    {
        public ContentRepository(IDbConnectionFactory f) : base(f) { }

        /* === Users === */
        public async Task<int> CreateUserAsync(CreateUserRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@Email", req.Email);
            p.Add("@PasswordHash", req.PasswordHash);
            p.Add("@Role", (byte)req.Role);
            p.Add("@UserId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            await conn.ExecuteAsync("dbo.sp_User_Create", p, commandType: System.Data.CommandType.StoredProcedure);
            return p.Get<int>("@UserId");
        }

        public async Task<UserDto?> GetUserAsync(int userId)
        {
            const string sql = @"SELECT UserId, Email, Role, CreatedAt FROM dbo.Users WHERE UserId=@id;";
            using var conn = Open();
            return await conn.QueryFirstOrDefaultAsync<UserDto>(sql, new { id = userId });
        }

        public async Task<IEnumerable<UserDto>> ListUsersAsync()
        {
            const string sql = @"SELECT UserId, Email, Role, CreatedAt FROM dbo.Users ORDER BY CreatedAt DESC;";
            using var conn = Open();
            return await conn.QueryAsync<UserDto>(sql);
        }

        /* === Quizzes === */
        public async Task<int> CreateQuizAsync(CreateQuizRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@Title", req.Title);
            p.Add("@Description", req.Description);
            p.Add("@CreatedBy", req.CreatedBy);
            p.Add("@IsPublished", req.IsPublished);
            p.Add("@QuizId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            await conn.ExecuteAsync("dbo.sp_Quiz_Create", p, commandType: System.Data.CommandType.StoredProcedure);
            return p.Get<int>("@QuizId");
        }

        public async Task<bool> UpdateQuizAsync(int quizId, UpdateQuizRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@QuizId", quizId);
            p.Add("@Title", req.Title);
            p.Add("@Description", req.Description);
            var rows = await conn.ExecuteAsync("dbo.sp_Quiz_Update", p, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0; // proc raises on not found
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
        {
            using var conn = Open();
            var rows = await conn.ExecuteAsync("dbo.sp_Quiz_Delete", new { QuizId = quizId }, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        public async Task<bool> SetQuizPublishedAsync(int quizId, bool isPublished)
        {
            using var conn = Open();
            var rows = await conn.ExecuteAsync("dbo.sp_Quiz_SetPublished", new { QuizId = quizId, IsPublished = isPublished }, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        /* === Questions === */
        public async Task<int> CreateQuestionAsync(int quizId, CreateQuestionRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@QuizId", quizId);
            p.Add("@Text", req.Text);
            p.Add("@QuestionType", req.QuestionType);
            p.Add("@TimeLimitSec", req.TimeLimitSec);
            p.Add("@Points", req.Points);
            p.Add("@MediaUrl", req.MediaUrl);
            p.Add("@OrderNo", req.OrderNo);
            p.Add("@QuestionId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            await conn.ExecuteAsync("dbo.sp_Question_Create", p, commandType: System.Data.CommandType.StoredProcedure);
            return p.Get<int>("@QuestionId");
        }

        public async Task<bool> UpdateQuestionAsync(int quizId, int questionId, UpdateQuestionRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@QuestionId", questionId);
            p.Add("@Text", req.Text);
            p.Add("@QuestionType", req.QuestionType);
            p.Add("@TimeLimitSec", req.TimeLimitSec);
            p.Add("@Points", req.Points);
            p.Add("@MediaUrl", req.MediaUrl);
            var rows = await conn.ExecuteAsync("dbo.sp_Question_Update", p, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        public async Task<bool> DeleteQuestionAsync(int quizId, int questionId)
        {
            using var conn = Open();
            var rows = await conn.ExecuteAsync("dbo.sp_Question_Delete", new { QuestionId = questionId }, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        public async Task<bool> ReorderQuestionAsync(int quizId, int questionId, int newOrderNo)
        {
            using var conn = Open();
            var rows = await conn.ExecuteAsync("dbo.sp_Question_Reorder", new { QuizId = quizId, QuestionId = questionId, NewOrderNo = newOrderNo }, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        /* === Choices === */
        public async Task<int> CreateChoiceAsync(int questionId, CreateChoiceRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@QuestionId", questionId);
            p.Add("@Text", req.Text);
            p.Add("@IsCorrect", req.IsCorrect);
            p.Add("@OrderNo", req.OrderNo);
            p.Add("@ChoiceId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            await conn.ExecuteAsync("dbo.sp_Choice_Create", p, commandType: System.Data.CommandType.StoredProcedure);
            return p.Get<int>("@ChoiceId");
        }

        public async Task<bool> UpdateChoiceAsync(int questionId, int choiceId, UpdateChoiceRequest req)
        {
            using var conn = Open();
            var p = new DynamicParameters();
            p.Add("@ChoiceId", choiceId);
            p.Add("@Text", req.Text);
            p.Add("@IsCorrect", req.IsCorrect);
            p.Add("@OrderNo", req.OrderNo);
            var rows = await conn.ExecuteAsync("dbo.sp_Choice_Update", p, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        public async Task<bool> DeleteChoiceAsync(int questionId, int choiceId)
        {
            using var conn = Open();
            var rows = await conn.ExecuteAsync("dbo.sp_Choice_Delete", new { ChoiceId = choiceId }, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        public async Task<bool> ReorderChoiceAsync(int questionId, int choiceId, int newOrderNo)
        {
            using var conn = Open();
            var rows = await conn.ExecuteAsync("dbo.sp_Choice_Reorder", new { QuestionId = questionId, ChoiceId = choiceId, NewOrderNo = newOrderNo }, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        public async Task<bool> SetCorrectChoiceAsync(int questionId, int choiceId)
        {
            using var conn = Open();
            var rows = await conn.ExecuteAsync("dbo.sp_Choice_SetCorrect", new { QuestionId = questionId, ChoiceId = choiceId }, commandType: System.Data.CommandType.StoredProcedure);
            return rows >= 0;
        }

        public async Task<IEnumerable<QuizListDto>> GetQuizzesAsync(int? createdBy = null, bool? isPublished = null)
        {
            const string sql = @"
SELECT QuizId, Title, IsPublished, CreatedBy, CreatedAt, UpdatedAt
FROM dbo.Quizzes
WHERE (@createdBy IS NULL OR CreatedBy = @createdBy)
  AND (@isPublished IS NULL OR IsPublished = @isPublished)
ORDER BY CreatedAt DESC;";

            using var conn = Open();
            return await conn.QueryAsync<QuizListDto>(sql, new { createdBy, isPublished });
        }

        public async Task<QuizDetailDto?> GetQuizAsync(int quizId)
        {
            const string sql = @"
SELECT q.QuizId, q.Title, q.[Description], q.IsPublished, q.CreatedBy,
       q.CreatedAt, q.UpdatedAt,
       (SELECT COUNT(*) FROM dbo.Questions x WHERE x.QuizId = q.QuizId) AS QuestionCount
FROM dbo.Quizzes q
WHERE q.QuizId = @quizId;";

            using var conn = Open();
            return await conn.QueryFirstOrDefaultAsync<QuizDetailDto>(sql, new { quizId });
        }

        // ===== READ: Questions =====
        public async Task<IEnumerable<QuestionDto>> GetQuestionsWithChoicesAsync(int quizId)
        {
            const string sqlQ = @"
SELECT QuestionId, QuizId, [Text], QuestionType, TimeLimitSec, Points, MediaUrl, OrderNo
FROM dbo.Questions
WHERE QuizId = @quizId
ORDER BY OrderNo;";

            const string sqlC = @"
SELECT ChoiceId, QuestionId, [Text], IsCorrect, OrderNo
FROM dbo.Choices
WHERE QuestionId IN (SELECT QuestionId FROM dbo.Questions WHERE QuizId=@quizId)
ORDER BY QuestionId, OrderNo;";

            using var conn = Open();
            var q = (await conn.QueryAsync(sqlQ, new { quizId })).ToList();
            var c = (await conn.QueryAsync(sqlC, new { quizId })).ToList();

            var byQ = c.GroupBy(x => (int)x.QuestionId)
                       .ToDictionary(g => g.Key, g => g.Select(v =>
                           new ChoiceDto((int)v.ChoiceId, (string)v.Text, (bool)v.IsCorrect, (int)v.OrderNo)));

            return q.Select(v =>
            {
                var id = (int)v.QuestionId;
                byQ.TryGetValue(id, out var choices);
                return new QuestionDto(
                    id,
                    (int)v.QuizId,
                    (string)v.Text,
                    (byte)v.QuestionType,
                    (int)v.TimeLimitSec,
                    (int)v.Points,
                    (string?)v.MediaUrl,
                    (int)v.OrderNo,
                    choices ?? Enumerable.Empty<ChoiceDto>());
            });
        }

        public async Task<QuestionDto?> GetQuestionWithChoicesAsync(int questionId)
        {
            const string sqlQ = @"
SELECT QuestionId, QuizId, [Text], QuestionType, TimeLimitSec, Points, MediaUrl, OrderNo
FROM dbo.Questions WHERE QuestionId = @questionId;";

            const string sqlC = @"
SELECT ChoiceId, QuestionId, [Text], IsCorrect, OrderNo
FROM dbo.Choices WHERE QuestionId = @questionId
ORDER BY OrderNo;";

            using var conn = Open();
            var q = await conn.QueryFirstOrDefaultAsync(sqlQ, new { questionId });
            if (q == null) return null;

            var choices = await conn.QueryAsync(sqlC, new { questionId });
            var dtoChoices = choices.Select(v =>
                new ChoiceDto((int)v.ChoiceId, (string)v.Text, (bool)v.IsCorrect, (int)v.OrderNo));

            return new QuestionDto(
                (int)q.QuestionId,
                (int)q.QuizId,
                (string)q.Text,
                (byte)q.QuestionType,
                (int)q.TimeLimitSec,
                (int)q.Points,
                (string?)q.MediaUrl,
                (int)q.OrderNo,
                dtoChoices);
        }

        // ===== READ: Choices =====
        public async Task<IEnumerable<ChoiceDto>> GetChoicesAsync(int questionId)
        {
            const string sql = @"
SELECT ChoiceId, [Text], IsCorrect, OrderNo
FROM dbo.Choices
WHERE QuestionId = @questionId
ORDER BY OrderNo;";

            using var conn = Open();
            var rows = await conn.QueryAsync(sql, new { questionId });
            return rows.Select(v => new ChoiceDto((int)v.ChoiceId, (string)v.Text, (bool)v.IsCorrect, (int)v.OrderNo));
        }

        public async Task<ChoiceDto?> GetChoiceAsync(int choiceId)
        {
            const string sql = @"
SELECT ChoiceId, [Text], IsCorrect, OrderNo
FROM dbo.Choices WHERE ChoiceId = @choiceId;";

            using var conn = Open();
            var v = await conn.QueryFirstOrDefaultAsync(sql, new { choiceId });
            return v == null ? null : new ChoiceDto((int)v.ChoiceId, (string)v.Text, (bool)v.IsCorrect, (int)v.OrderNo);
        }
    }
}