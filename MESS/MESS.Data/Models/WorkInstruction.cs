using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace MESS.Data.Models;

/// <summary>
/// Represents a work instruction entity
/// Details contain title, version, status, and associated nodes and products.
/// </summary>
public class WorkInstruction : AuditableEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the work instruction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the work instruction. This field is required.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the version of the work instruction. This field is optional.
    /// </summary>
    public string? Version { get; set; }
    
    /// <summary>
    /// Gets or sets the id of the original work instruction in the version history. This allows for lineage tracking
    /// and is nullable.
    /// </summary>
    public int? OriginalId { get; set; }
    
    /// <summary>
    /// Gets or sets the original work instruction in the version history.
    /// </summary>
    [ForeignKey(nameof(OriginalId))]
    public WorkInstruction? Original { get; set; }
    
    // Helps identify the most recent version in a chain
    /// <summary>
    /// Gets or sets a value indicating whether a work instruction is its latest version.
    /// </summary>
    public bool IsLatest { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the work instruction is active.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this work instruction should have a QR code generated when a production
    /// log is completed for it.
    /// </summary>
    public bool ShouldGenerateQrCode { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this work instruction collects a product serial number.
    /// </summary>
    public bool CollectsProductSerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the list of nodes associated with the work instruction.
    /// </summary>
    public List<WorkInstructionNode> Nodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of products associated with the work instruction.
    /// </summary>
    public List<Product> Products { get; set; } = [];
}

/// <summary>
/// Validator for the <see cref="WorkInstruction"/> class.
/// Ensures that the properties of a work instruction meet the required validation rules.
/// </summary>
public class WorkInstructionValidator : AbstractValidator<WorkInstruction>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkInstructionValidator"/> class.
    /// Defines validation rules for the <see cref="WorkInstruction"/> entity.
    /// </summary>
    public WorkInstructionValidator()
    {
        RuleFor(x => x.Title)
            .NotNull()
            .NotEmpty()
            .Length(1, 2048)
            .WithMessage("Work Instruction Title length must be between 1 and 2048 characters.");
    }
}