using Microsoft.AspNetCore.Mvc;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/players")]
    public class PlayersAdminController : ControllerBase
    {
        private readonly IQueryRepository _repo;
        public PlayersAdminController(IQueryRepository repo) => _repo = repo;

        public record KickPlayerRequest(Guid GameSessionId);

        // Kick a player from a session
        [HttpPost("{playerId:int}/kick")]
        public async Task<IActionResult> Kick([FromRoute] int playerId, [FromBody] KickPlayerRequest req)
        {
            var ok = await _repo.KickPlayerAsync(playerId, req.GameSessionId);
            return ok ? NoContent() : NotFound();
        }

        public record UnkickPlayerRequest(Guid GameSessionId);

        [HttpPost("{playerId:int}/unkick")]
        public async Task<IActionResult> Unkick([FromRoute] int playerId, [FromBody] UnkickPlayerRequest req)
        {
            var ok = await _repo.UnkickPlayerAsync(playerId, req.GameSessionId);
            return ok ? NoContent() : NotFound();
        }
    }
}