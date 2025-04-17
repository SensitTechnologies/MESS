using FluentValidation;

namespace MESS.Data.Models;

/// <summary>
/// Represents an abstract base class for entities that are auditable.
/// Contains properties for tracking creation and modification metadata.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = "";

    /// <summary>
    /// Gets or sets the timestamp when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    public string LastModifiedBy { get; set; } = "";

    /// <summary>
    /// Gets or sets the timestamp when the entity was last modified.
    /// </summary>
    public DateTimeOffset LastModifiedOn { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Validator for the <see cref="AuditableEntity"/> class.
/// Ensures that the properties of the entity meet the required validation rules.
/// </summary>
public class AuditableEntityValidator : AbstractValidator<AuditableEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditableEntityValidator"/> class.
    /// Defines validation rules for the <see cref="AuditableEntity"/> properties.
    /// </summary>
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