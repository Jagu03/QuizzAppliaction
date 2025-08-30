using QuizApi.Dtos;

namespace QuizApi.Repositories
{
    public interface IAnswerRepository
    {
        Task<bool> SubmitAsync(SubmitAnswerRequest req);
    }
}
