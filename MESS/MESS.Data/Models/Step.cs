using FluentValidation;

namespace MESS.Data.Models;

public class Step : AuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Body { get; set; }
    public bool Success { get; set; }
    public DateTimeOffset SubmitTime { get; set; }
    public List<Part>? PartsNeeded { get; set; }
}

public class StepListValidator : AbstractValidator<List<Step>>
{
    public StepListValidator()
    {
        RuleFor(steps => steps)
            .NotEmpty()
            .Must(steps =>
            {
                for (int i = 1; i < steps.Count; i++)
                {
                    if (steps[i].SubmitTime < steps[i - 1].SubmitTime)
                    {
                        return false;
                    }
                }
                return true;
            })
            .WithMessage("Steps must be in ascending order.");
    }
}