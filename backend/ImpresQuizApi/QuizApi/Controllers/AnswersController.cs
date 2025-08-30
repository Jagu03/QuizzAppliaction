using Microsoft.AspNetCore.Mvc;
using QuizApi.Dtos;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/answers")]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerRepository _repo;
        public AnswersController(IAnswerRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<ActionResult<SubmitAnswerResponse>> Submit([FromBody] SubmitAnswerRequest req)
        {
            var ok = await _repo.SubmitAsync(req);
            return Ok(new SubmitAnswerResponse(ok));
        }
    }
}
