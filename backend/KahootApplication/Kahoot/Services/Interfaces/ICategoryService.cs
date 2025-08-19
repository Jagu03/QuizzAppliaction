using Kahoot.Models.Category;

namespace Kahoot.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> FetchCategoriesAsync();
        Task<CategoryResultModel> MergeCategoryAsync(Category model);
        Task<CategoryResultModel> DeleteCategoryAsync(int categoryId);
    }
}
