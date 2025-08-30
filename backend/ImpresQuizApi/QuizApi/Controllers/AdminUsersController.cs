using Microsoft.AspNetCore.Mvc;
using QuizApi.Dtos;
using QuizApi.Repositories;

namespace QuizApi.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IContentRepository _repo;
        public AdminUsersController(IContentRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
        {
            var id = await _repo.CreateUserAsync(req);
            var dto = await _repo.GetUserAsync(id);
            return CreatedAtAction(nameof(Get), new { id }, dto);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto?>> Get([FromRoute] int id)
        {
            var dto = await _repo.GetUserAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> List() => Ok(await _repo.ListUsersAsync());
    }
}
