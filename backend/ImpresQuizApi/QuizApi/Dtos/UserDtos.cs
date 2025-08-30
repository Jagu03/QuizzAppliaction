namespace QuizApi.Dtos
{
    public record CreateUserRequest(string Email, string PasswordHash, byte Role = 1);
    public record UserDto(int UserId, string Email, byte Role, DateTime CreatedAt);
}