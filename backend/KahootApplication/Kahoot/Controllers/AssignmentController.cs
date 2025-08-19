using Kahoot.Models.Assignment;
using Kahoot.Models.Common;
using Kahoot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kahoot.Controllers
{
    [ApiController]
    [Route("api/assignments")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignments;
        public AssignmentController(IAssignmentService assignments) => _assignments = assignments;

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] PublishAssignmentDto dto)
        {
            var id = await _assignments.PublishAsync(dto);
            return Ok(new ApiResponse<object>(true, "Assignment published", new { assignmentId = id }));
        }

        [HttpPost("publish-for-class")]
        public async Task<IActionResult> PublishForClass([FromBody] PublishForClassDto dto)
        {
            var (assignmentId, groupId) = await _assignments.PublishForClassAsync(dto);
            return Ok(new ApiResponse<object>(true, "Assignment published for class", new { assignmentId, groupId }));
        }

    }
}
