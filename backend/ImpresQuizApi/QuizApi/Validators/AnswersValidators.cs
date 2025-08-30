using FluentValidation;
using QuizApi.Dtos;

namespace QuizApi.Validators
{
    public class SubmitAnswerRequestValidator : AbstractValidator<SubmitAnswerRequest>
    {
        public SubmitAnswerRequestValidator()
        {
            RuleFor(x => x.GameSessionId).NotEmpty();
            RuleFor(x => x.PlayerId).GreaterThan(0);
            RuleFor(x => x.QuestionId).GreaterThan(0);
            RuleFor(x => x.ChoiceId).GreaterThan(0);
            RuleFor(x => x.TimeTakenMs).GreaterThanOrEqualTo(0);
        }
    }
}
