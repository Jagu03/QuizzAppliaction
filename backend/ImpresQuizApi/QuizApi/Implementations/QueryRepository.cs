using Dapper;
using QuizApi.Dtos;
using QuizApi.Infrastructure;
using QuizApi.Repositories;

namespace QuizApi.Implementations
{
    public class QueryRepository : RepositoryBase, IQueryRepository
    {
        public QueryRepository(IDbConnectionFactory f) : base(f) { }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsWithChoicesAsync(int quizId)
        {
            const string sqlQ = @"
SELECT q.QuestionId, q.QuizId, q.[Text], q.QuestionType, q.TimeLimitSec, q.Points, q.MediaUrl, q.OrderNo
FROM dbo.Questions q
WHERE q.QuizId = @quizId
ORDER BY q.OrderNo;";

            const string sqlC = @"
SELECT c.ChoiceId, c.QuestionId, c.[Text], c.IsCorrect, c.OrderNo
FROM dbo.Choices c
INNER JOIN dbo.Questions q ON q.QuestionId = c.QuestionId
WHERE q.QuizId = @quizId
ORDER BY q.QuestionId, c.OrderNo;";

            using var conn = Open();
            var questions = (await conn.QueryAsync(sqlQ, new { quizId })).ToList();
            var choices = (await conn.QueryAsync(sqlC, new { quizId })).ToList();

            // project to DTOs
            var choicesByQ = choices.GroupBy(x => (int)x.QuestionId)
                                    .ToDictionary(g => g.Key, g => g.Select(c =>
                                        new ChoiceDto((int)c.ChoiceId, (string)c.Text, (bool)c.IsCorrect, (int)c.OrderNo)));

            var dtos = questions.Select(q =>
            {
                var qid = (int)q.QuestionId;
                choicesByQ.TryGetValue(qid, out var cs);
                return new QuestionDto(
                    qid,
                    (int)q.QuizId,
                    (string)q.Text,
                    (byte)q.QuestionType,
                    (int)q.TimeLimitSec,
                    (int)q.Points,
                    (string?)q.MediaUrl,
                    (int)q.OrderNo,
                    cs ?? Enumerable.Empty<ChoiceDto>()
                );
            });

            return dtos;
        }

        public async Task<SessionStatusDto?> GetSessionAsync(Guid sessionId)
        {
            const string sql = @"
SELECT GameSessionId, QuizId, HostUserId, PinCode, [Status], StartedAt, EndedAt, CreatedAt
FROM dbo.GameSessions WHERE GameSessionId = @sessionId;";

            using var conn = Open();
            return await conn.QueryFirstOrDefaultAsync<SessionStatusDto>(sql, new { sessionId });
        }

        public async Task<IEnumerable<PlayerRow>> GetPlayersAsync(Guid sessionId)
        {
            const string sql = @"
SELECT PlayerId, DisplayName, IsKicked, JoinedAt
FROM dbo.Players
WHERE GameSessionId = @sessionId
ORDER BY JoinedAt ASC;";

            using var conn = Open();
            return await conn.QueryAsync<PlayerRow>(sql, new { sessionId });
        }

        public async Task<IEnumerable<QuestionStatRow>> GetQuestionStatsAsync(Guid sessionId)
        {
            const string sql = @"
SELECT GameSessionId, QuestionId, OrderNo,
       TotalSubmissions, CorrectSubmissions, AvgTimeTakenMs
FROM dbo.vw_QuestionStats
WHERE GameSessionId = @sessionId
ORDER BY OrderNo;";

            using var conn = Open();
            return await conn.QueryAsync<QuestionStatRow>(sql, new { sessionId });
        }

        public async Task<IEnumerable<PlayerAnswerRow>> GetPlayerAnswersAsync(Guid sessionId, int playerId)
        {
            const string sql = @"
SELECT PlayerAnswerId, GameSessionId, PlayerId, QuestionId, ChoiceId,
       IsCorrect, TimeTakenMs, ScoreAwarded, SubmittedAt
FROM dbo.PlayerAnswers
WHERE GameSessionId = @sessionId AND PlayerId = @playerId
ORDER BY SubmittedAt ASC;";

            using var conn = Open();
            return await conn.QueryAsync<PlayerAnswerRow>(sql, new { sessionId, playerId });
        }

        public async Task<bool> KickPlayerAsync(int playerId, Guid sessionId)
        {
            const string sql = @"
UPDATE dbo.Players
SET IsKicked = 1
WHERE PlayerId = @playerId AND GameSessionId = @sessionId AND IsKicked = 0;";

            using var conn = Open();
            var rows = await conn.ExecuteAsync(sql, new { playerId, sessionId });
            return rows > 0;
        }

        public async Task<SessionStatusDto?> GetActiveSessionByPinAsync(string pinCode)
        {
            const string sql = @"
SELECT TOP (1) GameSessionId, QuizId, HostUserId, PinCode, [Status], StartedAt, EndedAt, CreatedAt
FROM dbo.GameSessions
WHERE PinCode = @pin AND EndedAt IS NULL
ORDER BY CreatedAt DESC;";
            using var conn = Open();
            return await conn.QueryFirstOrDefaultAsync<SessionStatusDto>(sql, new { pin = pinCode });
        }

        public async Task<bool> UnkickPlayerAsync(int playerId, Guid sessionId)
        {
            const string sql = @"
UPDATE dbo.Players
   SET IsKicked = 0
 WHERE PlayerId = @playerId AND GameSessionId = @sessionId AND IsKicked = 1;";
            using var conn = Open();
            return (await conn.ExecuteAsync(sql, new { playerId, sessionId })) > 0;
        }
        public async Task<IEnumerable<QuizListDto>> GetQuizzesAsync(int? createdBy)
        {
            const string sql = @"
SELECT QuizId, Title, IsPublished, CreatedBy, CreatedAt, UpdatedAt
FROM dbo.Quizzes
WHERE (@createdBy IS NULL OR CreatedBy = @createdBy)
ORDER BY CreatedAt DESC;";
            using var conn = Open();
            return await conn.QueryAsync<QuizListDto>(sql, new { createdBy });
        }

        public async Task<IEnumerable<QuestionAnswerDto>> GetAnswersByQuestionAsync(Guid sessionId, int questionId)
        {
            const string sql = @"
SELECT pa.PlayerAnswerId, pa.GameSessionId, pa.PlayerId, p.DisplayName AS PlayerName,
       pa.QuestionId, pa.ChoiceId, c.[Text] AS ChoiceText, pa.IsCorrect,
       pa.TimeTakenMs, pa.ScoreAwarded, pa.SubmittedAt
FROM dbo.PlayerAnswers pa
JOIN dbo.Players p   ON p.PlayerId = pa.PlayerId
LEFT JOIN dbo.Choices c ON c.ChoiceId = pa.ChoiceId
WHERE pa.GameSessionId = @sid AND pa.QuestionId = @qid
ORDER BY pa.SubmittedAt ASC;";
            using var conn = Open();
            return await conn.QueryAsync<QuestionAnswerDto>(sql, new { sid = sessionId, qid = questionId });
        }

        public async Task<IEnumerable<ChoiceSummaryDto>> GetChoiceSummaryAsync(Guid sessionId, int questionId)
        {
            const string sql = @"
SELECT c.ChoiceId, c.[Text],
       SUM(CASE WHEN pa.ChoiceId = c.ChoiceId THEN 1 ELSE 0 END) AS Cnt
FROM dbo.Choices c
JOIN dbo.Questions q ON q.QuestionId = c.QuestionId
LEFT JOIN dbo.PlayerAnswers pa
  ON pa.QuestionId = q.QuestionId AND pa.GameSessionId = @sid
WHERE q.QuestionId = @qid
GROUP BY c.ChoiceId, c.[Text]
ORDER BY MIN(c.OrderNo);";
            using var conn = Open();
            var rows = (await conn.QueryAsync<(int ChoiceId, string Text, int Cnt)>(sql, new { sid = sessionId, qid = questionId })).ToList();
            var total = rows.Sum(r => r.Cnt);
            return rows.Select(r => new ChoiceSummaryDto(r.ChoiceId, r.Text, r.Cnt, total == 0 ? 0 : Math.Round((double)r.Cnt * 100.0 / total, 2)));
        }

        public async Task<IEnumerable<PlayerRow>> GetActivePlayersAsync(Guid sessionId)
        {
            const string sql = @"
SELECT PlayerId, DisplayName, IsKicked, JoinedAt
FROM dbo.Players
WHERE GameSessionId = @sessionId AND IsKicked = 0
ORDER BY JoinedAt ASC;";
            using var conn = Open();
            return await conn.QueryAsync<PlayerRow>(sql, new { sessionId });
        }
    }
}
