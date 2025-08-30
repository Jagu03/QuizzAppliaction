using FluentValidation;
using QuizApi.Dtos;

namespace QuizApi.Validators
{
    public class JoinPlayerRequestValidator : AbstractValidator<JoinPlayerRequest>
    {
        public JoinPlayerRequestValidator()
        {
            RuleFor(x => x.PinCode).NotEmpty().Length(6);
            RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(50);
        }
    }
}
