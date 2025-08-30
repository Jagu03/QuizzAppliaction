using FluentValidation;
using QuizApi.Dtos;

namespace QuizApi.Validators
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.PasswordHash).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Role).InclusiveBetween((byte)0, (byte)1);
        }
    }
}