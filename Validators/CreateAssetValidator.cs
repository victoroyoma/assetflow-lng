using buildone.Data;
using buildone.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace buildone.Validators;

/// <summary>
/// Validator for asset creation
/// </summary>
public class CreateAssetValidator : AbstractValidator<CreateAssetDto>
{
    private readonly ApplicationDbContext _context;

    public CreateAssetValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.AssetTag)
            .NotEmpty().WithMessage("Asset tag is required")
            .MaximumLength(50).WithMessage("Asset tag cannot exceed 50 characters")
            .MustAsync(BeUniqueAssetTag).WithMessage("Asset tag already exists");

        RuleFor(x => x.PcId)
            .NotEmpty().WithMessage("PC ID is required")
            .MaximumLength(50).WithMessage("PC ID cannot exceed 50 characters")
            .MustAsync(BeUniquePcId).WithMessage("PC ID already exists");

        RuleFor(x => x.SerialNumber)
            .MaximumLength(100).WithMessage("Serial number cannot exceed 100 characters")
            .MustAsync(BeUniqueSerialNumber).WithMessage("Serial number already exists")
            .When(x => !string.IsNullOrEmpty(x.SerialNumber));

        RuleFor(x => x.Brand)
            .MaximumLength(50).WithMessage("Brand cannot exceed 50 characters");

        RuleFor(x => x.Model)
            .MaximumLength(100).WithMessage("Model cannot exceed 100 characters");

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x.WarrantyExpiry)
            .GreaterThan(DateTime.UtcNow).WithMessage("Warranty expiry must be a future date")
            .When(x => x.WarrantyExpiry.HasValue);

        RuleFor(x => x.AssignedEmployeeId)
            .MustAsync(EmployeeExists).WithMessage("Selected employee does not exist")
            .When(x => x.AssignedEmployeeId.HasValue);

        RuleFor(x => x.DepartmentId)
            .MustAsync(DepartmentExists).WithMessage("Selected department does not exist")
            .When(x => x.DepartmentId.HasValue);
    }

    private async Task<bool> BeUniqueAssetTag(string assetTag, CancellationToken cancellationToken)
    {
        return !await _context.Assets.AnyAsync(a => a.AssetTag == assetTag, cancellationToken);
    }

    private async Task<bool> BeUniquePcId(string pcId, CancellationToken cancellationToken)
    {
        return !await _context.Assets.AnyAsync(a => a.PcId == pcId, cancellationToken);
    }

    private async Task<bool> BeUniqueSerialNumber(string? serialNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(serialNumber)) return true;
        return !await _context.Assets.AnyAsync(a => a.SerialNumber == serialNumber, cancellationToken);
    }

    private async Task<bool> EmployeeExists(int? employeeId, CancellationToken cancellationToken)
    {
        if (!employeeId.HasValue) return true;
        return await _context.Employees.AnyAsync(e => e.Id == employeeId.Value, cancellationToken);
    }

    private async Task<bool> DepartmentExists(int? departmentId, CancellationToken cancellationToken)
    {
        if (!departmentId.HasValue) return true;
        return await _context.Departments.AnyAsync(d => d.Id == departmentId.Value, cancellationToken);
    }
}
