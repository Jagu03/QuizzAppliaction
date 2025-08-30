using Microsoft.AspNetCore.Mvc;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionsQueryController : ControllerBase
    {
        private readonly IQueryRepository _repo;
        public SessionsQueryController(IQueryRepository repo) => _repo = repo;

        // Session status
        [HttpGet("{id:guid}/status")]
        public async Task<IActionResult> Status([FromRoute] Guid id)
            => Ok(await _repo.GetSessionAsync(id));

        // Players list
        [HttpGet("{id:guid}/players")]
        public async Task<IActionResult> Players([FromRoute] Guid id)
            => Ok(await _repo.GetPlayersAsync(id));

        // Per-question stats (from view)
        [HttpGet("{id:guid}/question-stats")]
        public async Task<IActionResult> QuestionStats([FromRoute] Guid id)
            => Ok(await _repo.GetQuestionStatsAsync(id));

        // Answers of a player within this session
        [HttpGet("{id:guid}/answers/by-player/{playerId:int}")]
        public async Task<IActionResult> PlayerAnswers([FromRoute] Guid id, [FromRoute] int playerId)
            => Ok(await _repo.GetPlayerAnswersAsync(id, playerId));

        [HttpGet("by-pin/{pin:length(6)}/status")]
        public async Task<IActionResult> StatusByPin([FromRoute] string pin)
        {
            var s = await _repo.GetActiveSessionByPinAsync(pin);
            return s is null ? NotFound() : Ok(s);
        }

        [HttpGet("{id:guid}/answers/by-question/{questionId:int}")]
        public async Task<IActionResult> AnswersByQuestion([FromRoute] Guid id, [FromRoute] int questionId)
    => Ok(await _repo.GetAnswersByQuestionAsync(id, questionId));

        [HttpGet("{id:guid}/answers/summary/{questionId:int}")]
        public async Task<IActionResult> AnswerSummary([FromRoute] Guid id, [FromRoute] int questionId)
            => Ok(await _repo.GetChoiceSummaryAsync(id, questionId));

        [HttpGet("{id:guid}/players/active")]
        public async Task<IActionResult> ActivePlayers([FromRoute] Guid id)
    => Ok(await _repo.GetActivePlayersAsync(id));
    }
}
