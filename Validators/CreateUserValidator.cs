using buildone.Data;
using buildone.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace buildone.Validators;

/// <summary>
/// Validator for user creation
/// </summary>
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    private readonly ApplicationDbContext _context;

    public CreateUserValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
            .MustAsync(BeUniqueEmail).WithMessage("Email is already in use");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[\d\s\-\+\(\)]*$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.EmployeeId)
            .MustAsync(EmployeeExists).WithMessage("Selected employee does not exist")
            .MustAsync(EmployeeNotLinked).WithMessage("Selected employee is already linked to another user")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role must be selected")
            .Must(roles => roles.All(r => !string.IsNullOrWhiteSpace(r)))
            .WithMessage("Invalid role selection");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    private async Task<bool> EmployeeExists(int? employeeId, CancellationToken cancellationToken)
    {
        if (!employeeId.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == employeeId.Value, cancellationToken);
    }

    private async Task<bool> EmployeeNotLinked(int? employeeId, CancellationToken cancellationToken)
    {
        if (!employeeId.HasValue) return true;
        return !await _context.Users.AnyAsync(u => u.EmployeeId == employeeId.Value, cancellationToken);
    }
}
