using QuizApi.Dtos;

namespace QuizApi.Repositories
{
    public interface IPlayerRepository
    {
        Task<JoinPlayerResponse> JoinByPinAsync(string pinCode, string displayName);
    }
}
