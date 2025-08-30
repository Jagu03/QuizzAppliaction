using Microsoft.AspNetCore.Mvc;
using QuizApi.Dtos;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/admin/quizzes/{quizId:int}/questions")]
    public class AdminQuestionsController : ControllerBase
    {
        private readonly IContentRepository _repo;
        public AdminQuestionsController(IContentRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<ActionResult<CreateQuestionResponse>> Create([FromRoute] int quizId, [FromBody] CreateQuestionRequest req)
        {
            var id = await _repo.CreateQuestionAsync(quizId, req);
            return Ok(new CreateQuestionResponse(id));
        }

        [HttpPut("{questionId:int}")]
        public async Task<IActionResult> Update([FromRoute] int quizId, [FromRoute] int questionId, [FromBody] UpdateQuestionRequest req)
            => await _repo.UpdateQuestionAsync(quizId, questionId, req) ? NoContent() : NotFound();

        [HttpDelete("{questionId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int quizId, [FromRoute] int questionId)
            => await _repo.DeleteQuestionAsync(quizId, questionId) ? NoContent() : NotFound();

        [HttpPost("{questionId:int}/reorder")]
        public async Task<IActionResult> Reorder([FromRoute] int quizId, [FromRoute] int questionId, [FromBody] ReorderQuestionRequest req)
            => await _repo.ReorderQuestionAsync(quizId, questionId, req.NewOrderNo) ? NoContent() : NotFound();

          // GET /api/admin/quizzes/{quizId}/questions
        [HttpGet]
        public async Task<IActionResult> List([FromRoute] int quizId)
            => Ok(await _repo.GetQuestionsWithChoicesAsync(quizId));

        // GET /api/admin/quizzes/{quizId}/questions/{questionId}
        [HttpGet("{questionId:int}")]
        public async Task<IActionResult> Get([FromRoute] int quizId, [FromRoute] int questionId)
        {
            var dto = await _repo.GetQuestionWithChoicesAsync(questionId);
            return dto is null || dto.QuizId != quizId ? NotFound() : Ok(dto);
        }
    }
}
