using Microsoft.AspNetCore.Mvc;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardRepository _repo;
        public LeaderboardController(ILeaderboardRepository repo) => _repo = repo;

        [HttpGet("{gameSessionId:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid gameSessionId)
            => Ok(await _repo.GetAsync(gameSessionId));
    }
}
