using Kahoot.Models.Common;
using Kahoot.Models.Group;
using Kahoot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kahoot.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groups;
        public GroupController(IGroupService groups) => _groups = groups;

        [HttpPost("class")]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassGroupDto dto)
        {
            var id = await _groups.CreateGroupForClassAsync(dto);
            return Ok(new ApiResponse<object>(true, "Group created", new { groupId = id }));
        }

        [HttpPost("custom")]
        public async Task<IActionResult> CreateCustom([FromBody] CreateCustomGroupDto dto)
        {
            var id = await _groups.CreateCustomGroupAsync(dto);
            return Ok(new ApiResponse<object>(true, "Group created", new { groupId = id }));
        }
    }
}