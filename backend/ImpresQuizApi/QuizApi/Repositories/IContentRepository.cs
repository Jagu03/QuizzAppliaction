using QuizApi.Dtos;

namespace QuizApi.Repositories
{
    public interface IContentRepository
    {
        // Users
        Task<int> CreateUserAsync(CreateUserRequest req);
        Task<UserDto?> GetUserAsync(int userId);
        Task<IEnumerable<UserDto>> ListUsersAsync();

        // Quizzes
        Task<int> CreateQuizAsync(CreateQuizRequest req);
        Task<bool> UpdateQuizAsync(int quizId, UpdateQuizRequest req);
        Task<bool> DeleteQuizAsync(int quizId);
        Task<bool> SetQuizPublishedAsync(int quizId, bool isPublished);

        // Questions
        Task<int> CreateQuestionAsync(int quizId, CreateQuestionRequest req);
        Task<bool> UpdateQuestionAsync(int quizId, int questionId, UpdateQuestionRequest req);
        Task<bool> DeleteQuestionAsync(int quizId, int questionId);
        Task<bool> ReorderQuestionAsync(int quizId, int questionId, int newOrderNo);

        // Choices
        Task<int> CreateChoiceAsync(int questionId, CreateChoiceRequest req);
        Task<bool> UpdateChoiceAsync(int questionId, int choiceId, UpdateChoiceRequest req);
        Task<bool> DeleteChoiceAsync(int questionId, int choiceId);
        Task<bool> ReorderChoiceAsync(int questionId, int choiceId, int newOrderNo);
        Task<bool> SetCorrectChoiceAsync(int questionId, int choiceId);

        // READ: Quizzes
        Task<IEnumerable<QuizListDto>> GetQuizzesAsync(int? createdBy = null, bool? isPublished = null);
        Task<QuizDetailDto?> GetQuizAsync(int quizId);

        // READ: Questions
        Task<IEnumerable<QuestionDto>> GetQuestionsWithChoicesAsync(int quizId);
        Task<QuestionDto?> GetQuestionWithChoicesAsync(int questionId);

        // READ: Choices
        Task<IEnumerable<ChoiceDto>> GetChoicesAsync(int questionId);
        Task<ChoiceDto?> GetChoiceAsync(int choiceId);
    }
}