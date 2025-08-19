using Kahoot.Models.Category;
using Kahoot.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Kahoot.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly string _connectionString;

        public CategoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Category>> FetchCategoriesAsync()
        {
            var categories = new List<Category>();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Quizz.FetchCategories", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(new Category
                        {
                            CategoryId = reader.GetInt32(0),
                            CategoryName = reader.GetString(1)
                        });
                    }
                }
            }
            return categories;
        }

        public async Task<CategoryResultModel> MergeCategoryAsync(Category model)
        {
            var result = new CategoryResultModel();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Quizz.MergeCategory", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@CategoryName", model.CategoryName ?? (object)DBNull.Value);
                var outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputParam);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                result.Result = outputParam.Value?.ToString();
            }
            return result;
        }

        public async Task<CategoryResultModel> DeleteCategoryAsync(int categoryId)
        {
            var result = new CategoryResultModel();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("Quizz.DeleteCategory", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                var outputParam = new SqlParameter("@Result", SqlDbType.NVarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputParam);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                result.Result = outputParam.Value?.ToString();
            }
            return result;
        }
    }
}
