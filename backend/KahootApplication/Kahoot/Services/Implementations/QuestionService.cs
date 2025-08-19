using Kahoot.Models.Category;
using Kahoot.Models.InsertQuestionRequest;
using Kahoot.Models.Option;
using Kahoot.Models.Question;
using Kahoot.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Kahoot.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public QuestionService(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> InsertQuestionAsync(InsertQuestionRequest request)
        {
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("Quizz.InsertQuestionWithOptionsAndCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);
            cmd.Parameters.AddWithValue("@QuestionText", request.QuestionText);
            cmd.Parameters.AddWithValue("@Option1", request.Option1);
            cmd.Parameters.AddWithValue("@Option2", request.Option2);
            cmd.Parameters.AddWithValue("@Option3", request.Option3);
            cmd.Parameters.AddWithValue("@Option4", request.Option4);
            cmd.Parameters.AddWithValue("@CorrectOptionNumber", request.CorrectOptionNumber);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return true;
        }
        public async Task<bool> UpdateQuestionAsync(UpdateQuestionRequest request)
        {
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("Quizz.UpdateQuestionWithOptionsAndCategory", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QuestionId", request.QuestionId);
            cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);
            cmd.Parameters.AddWithValue("@QuestionText", request.QuestionText);
            cmd.Parameters.AddWithValue("@Option1", request.Option1);
            cmd.Parameters.AddWithValue("@Option2", request.Option2);
            cmd.Parameters.AddWithValue("@Option3", request.Option3);
            cmd.Parameters.AddWithValue("@Option4", request.Option4);
            cmd.Parameters.AddWithValue("@CorrectOptionNumber", request.CorrectOptionNumber);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<string> DeleteQuestionAsync(int questionId)
        {
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("Quizz.DeleteQuestion", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QuestionId", questionId);
            var output = new SqlParameter("@Result", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return output.Value?.ToString() ?? "Unknown result";
        }

        public async Task<List<Question>> FetchQuestionsAsync(int? categoryId = null)
        {
            List<Question> result = new();
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("Quizz.FetchQuestions", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CategoryId", (object?)categoryId ?? DBNull.Value);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var map = new Dictionary<int, Question>();
            while (await reader.ReadAsync())
            {
                int qid = reader.GetInt32(0);
                if (!map.ContainsKey(qid))
                {
                    map[qid] = new Question
                    {
                        QuestionId = qid,
                        QuestionText = reader.GetString(1),
                        Options = new List<QuestionOption>(),
                        CorrectOptionId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        CategoryName = reader.GetString(5),
                        CategoryId = reader.GetInt32(6)
                    };
                }
                map[qid].Options.Add(new QuestionOption
                {
                    OptionId = reader.GetInt32(2),
                    OptionText = reader.GetString(3)
                });
            }
            return map.Values.ToList();
        }
        public async Task<Question?> GetQuestionAsync(int questionId)
        {
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("Quizz.GetQuestionById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QuestionId", questionId);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            Question? q = null;

            // header
            if (await reader.ReadAsync())
            {
                q = new Question
                {
                    QuestionId = reader.GetInt32(0),
                    QuestionText = reader.GetString(1),
                    CorrectOptionId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    CategoryId = reader.GetInt32(3),
                    CategoryName = reader.GetString(4),
                    Options = new List<QuestionOption>()
                };
            }

            if (q == null) return null;

            // options
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                q.Options.Add(new QuestionOption
                {
                    OptionId = reader.GetInt32(0),
                    OptionText = reader.GetString(1)
                });
            }
            return q;
        }

        public async Task<List<Question>> GetQuestionsByCategoryAsync(int categoryId)
        {
            List<Question> result = new();
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("Quizz.GetQuestionsByCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            Dictionary<int, Question> questionMap = new();

            while (await reader.ReadAsync())
            {
                int qid = reader.GetInt32(0);
                if (!questionMap.ContainsKey(qid))
                {
                    questionMap[qid] = new Question
                    {
                        QuestionId = qid,
                        QuestionText = reader.GetString(1),
                        Options = new List<QuestionOption>(),
                        CorrectOptionId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        CategoryName = reader.GetString(5)
                    };
                }

                questionMap[qid].Options.Add(new QuestionOption
                {
                    OptionId = reader.GetInt32(2),
                    OptionText = reader.GetString(3)
                });
            }

            return questionMap.Values.ToList();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("Quizz.GetAllCategories", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<Category> categories = new();
            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new Category
                {
                    CategoryId = reader.GetInt32(0),
                    CategoryName = reader.GetString(1)
                });
            }
            return categories;
        }
    }
}
