using Kahoot.Models.Common;
using Kahoot.Models.Quiz;
using Kahoot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Kahoot.Controllers
{
    [ApiController]
    [Route("api/quiz")]
    public class QuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _quiz;
        public QuizAttemptController(IQuizAttemptService quiz) => _quiz = quiz;

        [HttpGet("student/{studentId}/active")]
        public async Task<IActionResult> ActiveAssignments(int studentId)
        {
            var dt = await _quiz.FetchActiveAssignmentsForStudentAsync(studentId);

            // Map DataTable -> List<StudentActiveAssignmentDto>
            var rows = new List<StudentActiveAssignmentDto>();
            foreach (DataRow r in dt.Rows)
            {
                rows.Add(new StudentActiveAssignmentDto
                {
                    StudentAssignmentId = r.Field<int>("StudentAssignmentId"),
                    AssignmentId = r.Field<int>("AssignmentId"),
                    Title = r.Field<string>("Title") ?? string.Empty,
                    StartAt = r.Field<DateTime>("StartAt"),
                    EndAt = r.Field<DateTime>("EndAt"),
                    TimeLimitSeconds = r.IsNull("TimeLimitSeconds") ? null : r.Field<int?>("TimeLimitSeconds"),
                    MaxAttempts = r.Field<int>("MaxAttempts"),
                    AttemptCount = r.Field<int>("AttemptCount"),
                    CategoryId = r.Field<int>("CategoryId")
                });
            }

            return Ok(new ApiResponse<IEnumerable<StudentActiveAssignmentDto>>(true, "OK", rows));
        }

        [HttpPost("attempts/start")]
        public async Task<IActionResult> Start([FromBody] StartAttemptDto dto)
        {
            var (attemptId, qdt, odt) = await _quiz.StartAttemptAsync(dto);

            // questions
            var questions = new List<AttemptQuestionDto>();
            foreach (DataRow r in qdt.Rows)
            {
                questions.Add(new AttemptQuestionDto
                {
                    AttemptAnswerId = r.Field<int>("AttemptAnswerId"),
                    QuestionId = r.Field<int>("QuestionId"),
                    QuestionText = r.Field<string>("QuestionText") ?? string.Empty
                });
            }

            // options
            var options = new List<AttemptOptionDto>();
            foreach (DataRow r in odt.Rows)
            {
                options.Add(new AttemptOptionDto
                {
                    QuestionId = r.Field<int>("QuestionId"),
                    OptionId = r.Field<int>("OptionId"),
                    OptionText = r.Field<string>("OptionText") ?? string.Empty
                });
            }

            return Ok(new ApiResponse<object>(true, "Started", new
            {
                attemptId,
                questions,
                options
            }));
        }

        [HttpPost("attempts/answer")]
        public async Task<IActionResult> Answer([FromBody] AnswerDto dto)
        {
            await _quiz.SubmitAnswerAsync(dto);
            return Ok(new ApiResponse<object>(true, "Saved"));
        }

        [HttpPost("attempts/finish")]
        public async Task<IActionResult> Finish([FromBody] FinishAttemptDto dto)
        {
            var (score, total) = await _quiz.FinishAttemptAsync(dto);
            return Ok(new ApiResponse<object>(true, "Finished", new { score, total }));
        }
    }
}
