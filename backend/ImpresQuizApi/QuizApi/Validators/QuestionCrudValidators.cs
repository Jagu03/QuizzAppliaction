using FluentValidation;
using QuizApi.Dtos;

namespace QuizApi.Validators
{
    public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
    {
        public CreateQuestionRequestValidator()
        {
            RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
            RuleFor(x => x.QuestionType).Equal((byte)0);
            RuleFor(x => x.TimeLimitSec).InclusiveBetween(1, 600);
            RuleFor(x => x.Points).InclusiveBetween(0, 100000);
        }
    }
    public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
    {
        public UpdateQuestionRequestValidator()
        {
            RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
            RuleFor(x => x.QuestionType).Equal((byte)0);
            RuleFor(x => x.TimeLimitSec).InclusiveBetween(1, 600);
            RuleFor(x => x.Points).InclusiveBetween(0, 100000);
        }
    }
}