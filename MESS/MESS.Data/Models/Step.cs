using FluentValidation;

namespace MESS.Data.Models;

/// <summary>
/// Represents a step in a work instruction, inheriting from <see cref="WorkInstructionNode"/>.
/// </summary>
public class Step : WorkInstructionNode
{
    /// <summary>
    /// Gets or sets the name of the step. This property is required.
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the body of the step. This property is required.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the detailed body content of the step. This property is optional.
    /// </summary>
    public string? DetailedBody { get; set; }

    /// <summary>
    /// Gets or sets the list of primary media (image path + optional caption) associated with the step.
    /// </summary>
    public List<StepMediaItem> PrimaryMedia { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of secondary media (image path + optional caption) associated with the step.
    /// </summary>
    public List<StepMediaItem> SecondaryMedia { get; set; } = [];

    /// <summary>
    /// How operator notes (production log step attempts) behave for this step in My Production.
    /// </summary>
    public StepNotesConfiguration NotesConfiguration { get; set; } = StepNotesConfiguration.OptionalForFailure;
}

/// <summary>
/// Validator for the <see cref="Step"/> class. Validates each attached photo (path and caption).
/// </summary>
public class StepValidator : AbstractValidator<Step>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepValidator"/> class.
    /// </summary>
    public StepValidator()
    {
        RuleForEach(x => x.PrimaryMedia).SetValidator(new StepMediaItemValidator());
        RuleForEach(x => x.SecondaryMedia).SetValidator(new StepMediaItemValidator());
    }
}
