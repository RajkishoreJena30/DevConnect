using DevConnect.DTOs;
using FluentValidation;

namespace DevConnect.Validators
{
    // Validates RegisterDTO — runs before AuthController.Register()
    public class RegisterValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Must contain a number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a special character.");
        }
    }

    // Validates LoginDTO — runs before AuthController.Login()
    public class LoginValidator : AbstractValidator<LoginDTO>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}