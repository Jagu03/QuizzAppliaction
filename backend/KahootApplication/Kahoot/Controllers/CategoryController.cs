using Kahoot.Models.Category;
using Kahoot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kahoot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("fetch")]
        public async Task<IActionResult> FetchCategories()
        {
            var data = await _categoryService.FetchCategoriesAsync();
            return Ok(data);
        }

        [HttpPost("merge")]
        public async Task<IActionResult> MergeCategory([FromBody] Category model)
        {
            var result = await _categoryService.MergeCategoryAsync(model);
            return Ok(result);
        }

        [HttpDelete("delete/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var result = await _categoryService.DeleteCategoryAsync(categoryId);
            return Ok(result);
        }
    }
}