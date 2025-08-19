using Kahoot.Models.Quiz;
using System.Data;

namespace Kahoot.Services.Interfaces
{
    public interface IQuizAttemptService
    {
        Task<DataTable> FetchActiveAssignmentsForStudentAsync(int studentId);
        Task<(int attemptId, DataTable questions, DataTable options)> StartAttemptAsync(StartAttemptDto dto);
        Task SubmitAnswerAsync(AnswerDto dto);
        Task<(int score, int total)> FinishAttemptAsync(FinishAttemptDto dto);

        // Added missing method name and parameters
        // NEW: get an existing attempt's paper (questions + options) as DTOs (no DataTable serialization issues)
        Task<(int attemptId, List<AttemptQuestionDto> questions, List<AttemptOptionDto> options)>
            GetAttemptDetailsAsync(int attemptId);

    }
}
