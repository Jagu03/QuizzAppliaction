using Kahoot.Models.Category;
using Kahoot.Models.InsertQuestionRequest;
using Kahoot.Models.Question;

namespace Kahoot.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<bool> InsertQuestionAsync(InsertQuestionRequest request);
        Task<bool> UpdateQuestionAsync(UpdateQuestionRequest request);
        Task<string> DeleteQuestionAsync(int questionId);
        Task<List<Question>> FetchQuestionsAsync(int? categoryId = null);
        Task<Question?> GetQuestionAsync(int questionId);
        Task<List<Question>> GetQuestionsByCategoryAsync(int categoryId);
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
