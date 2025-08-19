using Kahoot.Models.Quiz;
using Kahoot.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace Kahoot.Services.Implementations
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly string _cs;

        public QuizAttemptService(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection missing.");
            if (string.IsNullOrWhiteSpace(_cs))
                throw new InvalidOperationException("Empty connection string for DefaultConnection.");
        }

        public async Task<DataTable> FetchActiveAssignmentsForStudentAsync(int studentId)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.FetchActiveAssignmentsForStudent", con)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            await con.OpenAsync();
            da.Fill(dt);
            return dt;
        }

        public async Task<(int attemptId, DataTable questions, DataTable options)> StartAttemptAsync(StartAttemptDto dto)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.StartAttempt", con)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@StudentId", dto.StudentId);
            cmd.Parameters.AddWithValue("@AssignmentId", dto.AssignmentId);
            var outId = new SqlParameter("@AttemptId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outId);

            await con.OpenAsync();
            using var da = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            da.Fill(ds);

            int attemptId = (int)outId.Value;
            var qs = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
            var ops = ds.Tables.Count > 1 ? ds.Tables[1] : new DataTable();
            return (attemptId, qs, ops);
        }

        public async Task SubmitAnswerAsync(AnswerDto dto)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.SubmitAnswer", con)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AttemptId", dto.AttemptId);
            cmd.Parameters.AddWithValue("@QuestionId", dto.QuestionId);
            cmd.Parameters.AddWithValue("@SelectedOptionId", dto.SelectedOptionId);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<(int score, int total)> FinishAttemptAsync(FinishAttemptDto dto)
        {
            using var con = new SqlConnection(_cs);
            using var cmd = new SqlCommand("Quizz.FinishAttempt", con)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AttemptId", dto.AttemptId);

            await con.OpenAsync();
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            int score = Convert.ToInt32(dt.Rows[0]["Score"]);
            int total = Convert.ToInt32(dt.Rows[0]["Total"]);
            return (score, total);
        }

        // NEW: returns plain DTO lists so you can safely serialize to JSON
        public async Task<(int attemptId, List<AttemptQuestionDto> questions, List<AttemptOptionDto> options)>
            GetAttemptDetailsAsync(int attemptId)
        {
            var questions = new List<AttemptQuestionDto>();
            var options = new List<AttemptOptionDto>();

            using var con = new SqlConnection(_cs);
            await con.OpenAsync();

            // 1) Questions in the order of AttemptAnswers (same as StartAttempt returns)
            using (var qcmd = new SqlCommand(@"
                SELECT aa.AttemptAnswerId, aa.QuestionId, q.QuestionText
                FROM Quizz.AttemptAnswers aa
                INNER JOIN Quizz.Questions q ON q.QuestionId = aa.QuestionId
                WHERE aa.AttemptId = @AttemptId
                ORDER BY aa.AttemptAnswerId;", con))
            {
                qcmd.Parameters.AddWithValue("@AttemptId", attemptId);
                using var rdr = await qcmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    questions.Add(new AttemptQuestionDto
                    {
                        AttemptAnswerId = rdr.GetInt32(0),
                        QuestionId = rdr.GetInt32(1),
                        QuestionText = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2)
                    });
                }
            }

            // 2) Options for those questions (no shuffle here; client can render as-is)
            using (var ocmd = new SqlCommand(@"
                SELECT o.QuestionId, o.OptionId, o.OptionText
                FROM Quizz.AttemptAnswers aa
                INNER JOIN Quizz.Options o ON o.QuestionId = aa.QuestionId
                WHERE aa.AttemptId = @AttemptId
                ORDER BY aa.AttemptAnswerId, o.OptionId;", con))
            {
                ocmd.Parameters.AddWithValue("@AttemptId", attemptId);
                using var rdr = await ocmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    options.Add(new AttemptOptionDto
                    {
                        QuestionId = rdr.GetInt32(0),
                        OptionId = rdr.GetInt32(1),
                        OptionText = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2)
                    });
                }
            }

            return (attemptId, questions, options);
        }
    }
}
