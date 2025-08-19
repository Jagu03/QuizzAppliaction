using Kahoot.Models.StudentAuth;
using Kahoot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kahoot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentAuthController : ControllerBase
    {
        private readonly IStudentAuthService _authService;
        private readonly ILogger<StudentAuthController> _logger;

        public StudentAuthController(
            IStudentAuthService authService,
            ILogger<StudentAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<StudentLoginResponse>> Login([FromBody] StudentLoginRequest request)
        {
            try
            {
                // Set default values if not provided
                request.IPAddr ??= HttpContext.Connection.RemoteIpAddress?.ToString();
                request.Browser ??= HttpContext.Request.Headers["User-Agent"].ToString();
                request.SessId ??= Guid.NewGuid().ToString();

                var response = await _authService.AuthenticateStudent(request);

                return response.Result switch
                {
                    1 => Ok(response), // Success
                    0 => Unauthorized(response), // Invalid credentials
                    2 => NotFound(response), // Student not found
                    _ => StatusCode(500, response) // Other errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during student authentication");
                return StatusCode(500, new StudentLoginResponse
                {
                    Result = 3,
                    Message = "An error occurred during authentication"
                });
            }
        }
    }
}