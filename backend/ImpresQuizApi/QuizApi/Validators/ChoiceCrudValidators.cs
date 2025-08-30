using FluentValidation;
using QuizApi.Dtos;

namespace QuizApi.Validators
{
    public class CreateChoiceRequestValidator : AbstractValidator<CreateChoiceRequest>
    {
        public CreateChoiceRequestValidator()
        {
            RuleFor(x => x.Text).NotEmpty().MaximumLength(200);
        }
    }
    public class UpdateChoiceRequestValidator : AbstractValidator<UpdateChoiceRequest>
    {
        public UpdateChoiceRequestValidator()
        {
            RuleFor(x => x.Text).NotEmpty().MaximumLength(200);
        }
    }
}