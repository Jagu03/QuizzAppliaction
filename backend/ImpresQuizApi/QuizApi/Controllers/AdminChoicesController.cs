using Microsoft.AspNetCore.Mvc;
using QuizApi.Dtos;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/admin/questions/{questionId:int}/choices")]
    public class AdminChoicesController : ControllerBase
    {
        private readonly IContentRepository _repo;
        public AdminChoicesController(IContentRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<ActionResult<CreateChoiceResponse>> Create([FromRoute] int questionId, [FromBody] CreateChoiceRequest req)
        {
            var id = await _repo.CreateChoiceAsync(questionId, req);
            return Ok(new CreateChoiceResponse(id));
        }

        [HttpPut("{choiceId:int}")]
        public async Task<IActionResult> Update([FromRoute] int questionId, [FromRoute] int choiceId, [FromBody] UpdateChoiceRequest req)
            => await _repo.UpdateChoiceAsync(questionId, choiceId, req) ? NoContent() : NotFound();

        [HttpDelete("{choiceId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int questionId, [FromRoute] int choiceId)
            => await _repo.DeleteChoiceAsync(questionId, choiceId) ? NoContent() : NotFound();

        [HttpPost("{choiceId:int}/reorder")]
        public async Task<IActionResult> Reorder([FromRoute] int questionId, [FromRoute] int choiceId, [FromBody] ReorderChoiceRequest req)
            => await _repo.ReorderChoiceAsync(questionId, choiceId, req.NewOrderNo) ? NoContent() : NotFound();

        [HttpPost("{choiceId:int}/set-correct")]
        public async Task<IActionResult> SetCorrect([FromRoute] int questionId, [FromRoute] int choiceId)
            => await _repo.SetCorrectChoiceAsync(questionId, choiceId) ? NoContent() : NotFound();

        // GET /api/admin/questions/{questionId}/choices
        [HttpGet]
        public async Task<IActionResult> List([FromRoute] int questionId)
            => Ok(await _repo.GetChoicesAsync(questionId));

        // GET /api/admin/questions/{questionId}/choices/{choiceId}
        [HttpGet("{choiceId:int}")]
        public async Task<IActionResult> Get([FromRoute] int questionId, [FromRoute] int choiceId)
        {
            var dto = await _repo.GetChoiceAsync(choiceId);
            return dto is null ? NotFound() : Ok(dto);
        }
    }
}
