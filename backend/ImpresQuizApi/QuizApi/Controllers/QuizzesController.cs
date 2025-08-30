using Microsoft.AspNetCore.Mvc;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/quizzes")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQueryRepository _repo;
        public QuizzesController(IQueryRepository repo) => _repo = repo;

        // Get all questions (with choices) for a quiz
        [HttpGet("{quizId:int}/questions")]
        public async Task<IActionResult> GetQuestions([FromRoute] int quizId)
            => Ok(await _repo.GetQuestionsWithChoicesAsync(quizId));

        [HttpGet] // /api/quizzes?createdBy=123
        public async Task<IActionResult> List([FromQuery] int? createdBy)
    => Ok(await _repo.GetQuizzesAsync(createdBy));
    }
}