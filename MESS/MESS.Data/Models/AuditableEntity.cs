using FluentValidation;

namespace MESS.Data.Models;

public abstract class AuditableEntity
{
    public required string CreatedBy { get; set; }
    public required DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    public required string LastModifiedBy { get; set; }
    public required DateTimeOffset LastModifiedOn { get; set; } = DateTimeOffset.Now;
}

public class AuditableEntityValidator : AbstractValidator<AuditableEntity>
{
    public AuditableEntityValidator()
    {
        RuleFor(x => x.CreatedBy)
            .NotNull()
            .NotEmpty()
            .Length(1, 512)
            .WithMessage("Created By character count must be between 1 and 512 characters.");

        RuleFor(x => x.CreatedOn)
            .NotNull()
            .NotEmpty();
        
        RuleFor(x => x.LastModifiedBy)
            .NotNull()
            .NotEmpty();
        
        RuleFor(x => x.LastModifiedOn)
            .NotNull()
            .GreaterThanOrEqualTo(x => x.CreatedOn)
            .WithMessage("Last Modified On must be after or equal to Created On.")
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Last Modified On must be before or equal to the current UTC time.");
        
    }
}