using FluentValidation;
using QuizApi.Dtos;

namespace QuizApi.Validators
{
    public class CreateQuizRequestValidator : AbstractValidator<CreateQuizRequest>
    {
        public CreateQuizRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.CreatedBy).GreaterThan(0);
        }
    }

    public class UpdateQuizRequestValidator : AbstractValidator<UpdateQuizRequest>
    {
        public UpdateQuizRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}