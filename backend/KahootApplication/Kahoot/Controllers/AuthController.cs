using Kahoot.Models.Auth;
using Kahoot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kahoot.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Fallbacks if missing
                request.IPAddr ??= HttpContext.Connection.RemoteIpAddress?.ToString();
                request.Browser ??= HttpContext.Request.Headers["User-Agent"].ToString();
                request.SessId ??= Guid.NewGuid().ToString();
                request.PCUSName ??= Environment.MachineName;

                var response = await _authService.AuthenticateUserCommonLogin(request);

                return response.Result switch
                {
                    1 => Ok(response),        // Successful login
                    3 => Unauthorized(response), // Valid user, no module access
                    _ => Unauthorized(response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return StatusCode(500, new LoginResponse
                {
                    Result = 5,
                    Message = $"Internal Server Error: {ex.Message}" // TEMP: show actual message
                });
            }
        }
    }

}