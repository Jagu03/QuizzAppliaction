using Microsoft.AspNetCore.Mvc;
using QuizApi.Dtos;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerRepository _repo;
        public PlayersController(IPlayerRepository repo) => _repo = repo;

        [HttpPost("join")]
        public async Task<ActionResult<JoinPlayerResponse>> Join([FromBody] JoinPlayerRequest req)
            => Ok(await _repo.JoinByPinAsync(req.PinCode, req.DisplayName));
    }
}
