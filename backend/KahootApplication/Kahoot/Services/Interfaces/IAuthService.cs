using Kahoot.Models.Auth;

namespace Kahoot.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateUserCommonLogin(LoginRequest request);
    }
}
