using Kahoot.Models.InsertQuestionRequest;
using Kahoot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kahoot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertQuestion([FromBody] InsertQuestionRequest request)
        {
            var success = await _questionService.InsertQuestionAsync(request);
            return success ? Ok("Inserted") : StatusCode(500, "Insert failed");
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetQuestionsByCategory(int categoryId)
        {
            var questions = await _questionService.GetQuestionsByCategoryAsync(categoryId);
            return Ok(questions);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _questionService.GetAllCategoriesAsync();
            return Ok(result);
        }

        // PUT /api/Question/update
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateQuestionRequest request)
         => (await _questionService.UpdateQuestionAsync(request)) ? Ok("Updated") : StatusCode(500, "Update failed");

        // DELETE /api/Question/5
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> Delete(int questionId)
            => Ok(new { result = await _questionService.DeleteQuestionAsync(questionId) });

        // GET /api/Question/5
        [HttpGet("{questionId}")]
        public async Task<IActionResult> Get(int questionId)
        {
            var q = await _questionService.GetQuestionAsync(questionId);
            return q is null ? NotFound() : Ok(q);
        }

        // GET /api/Question?categoryId=3   (optional)
        [HttpGet]
        public async Task<IActionResult> Fetch([FromQuery] int? categoryId = null)
            => Ok(await _questionService.FetchQuestionsAsync(categoryId));
    }
}