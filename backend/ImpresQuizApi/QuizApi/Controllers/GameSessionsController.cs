using Microsoft.AspNetCore.Mvc;
using QuizApi.Dtos;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/game-sessions")]
    public class GameSessionsController : ControllerBase
    {
        private readonly IGameSessionRepository _repo;
        public GameSessionsController(IGameSessionRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<ActionResult<CreateGameSessionResponse>> Create([FromBody] CreateGameSessionRequest req)
            => Ok(await _repo.CreateAsync(req.QuizId, req.HostUserId));

        [HttpPost("{id:guid}/start")]
        public async Task<IActionResult> Start([FromRoute] Guid id)
        { await _repo.StartAsync(id); return NoContent(); }

        [HttpPost("{id:guid}/end")]
        public async Task<IActionResult> End([FromRoute] Guid id)
        { await _repo.EndAsync(id); return NoContent(); }

        // Controllers/GameSessionsController.cs (add)
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] Guid id)
        { await _repo.CancelAsync(id); return NoContent(); }

    }
}
