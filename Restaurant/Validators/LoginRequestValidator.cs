using FluentValidation;
using Restaurant.Requests;

namespace Restaurant.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
