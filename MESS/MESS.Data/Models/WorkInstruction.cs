using FluentValidation;

namespace MESS.Data.Models;

public class WorkInstruction : AuditableEntity
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    // public required IdentityUser CreatedBy { get; set; } 
    public required List<Step> Steps { get; set; } 
    public required List<Documentation> RelatedDocumentation { get; set; }
}

public class WorkInstructionValidator : AbstractValidator<WorkInstruction>
{
    public WorkInstructionValidator()
    {
        RuleFor(x => x.Title)
            .NotNull()
            .NotEmpty()
            .Length(1, 2048)
            .WithMessage("Work Instruction Title length must be between 1 and 2048 characters.");
        
        RuleFor(x => x.SubTitle)
            .Length(1, 2048)
            .WithMessage("Work Instruction SubTitle length must be between 1 and 2048 characters.");
    }
}