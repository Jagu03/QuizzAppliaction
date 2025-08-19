using Kahoot.Models.StudentAuth;

namespace Kahoot.Services.Interfaces
{
    public interface IStudentAuthService
    {
        Task<StudentLoginResponse> AuthenticateStudent(StudentLoginRequest request);
    }
}
