using Microsoft.AspNetCore.Mvc;
using QuizApi.Dtos;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/admin/quizzes")]
    public class AdminQuizzesController : ControllerBase
    {
        private readonly IContentRepository _repo;
        public AdminQuizzesController(IContentRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<ActionResult<CreateQuizResponse>> Create([FromBody] CreateQuizRequest req)
        {
            var id = await _repo.CreateQuizAsync(req);
            return Ok(new CreateQuizResponse(id));
        }

        [HttpPut("{quizId:int}")]
        public async Task<IActionResult> Update([FromRoute] int quizId, [FromBody] UpdateQuizRequest req)
            => await _repo.UpdateQuizAsync(quizId, req) ? NoContent() : NotFound();

        [HttpDelete("{quizId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int quizId)
            => await _repo.DeleteQuizAsync(quizId) ? NoContent() : NotFound();

        [HttpPost("{quizId:int}/publish")]
        public async Task<IActionResult> Publish([FromRoute] int quizId, [FromBody] PublishQuizRequest req)
            => await _repo.SetQuizPublishedAsync(quizId, req.IsPublished) ? NoContent() : NotFound();

        // GET /api/admin/quizzes?createdBy=&isPublished=
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int? createdBy, [FromQuery] bool? isPublished)
            => Ok(await _repo.GetQuizzesAsync(createdBy, isPublished));

        // GET /api/admin/quizzes/{quizId}
        [HttpGet("{quizId:int}")]
        public async Task<IActionResult> Get([FromRoute] int quizId)
        {
            var dto = await _repo.GetQuizAsync(quizId);
            return dto is null ? NotFound() : Ok(dto);
        }
    }
}
