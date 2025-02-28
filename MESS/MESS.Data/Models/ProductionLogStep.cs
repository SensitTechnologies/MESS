using FluentValidation;

namespace MESS.Data.Models;

public class ProductionLogStep
{
    public int Id { get; set; }
    public int ProductionLogId { get; set; }
    public int WorkInstructionStepId { get; set; }
    public bool Success { get; set; } = false;
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset SubmitTime { get; set; }
    
    public ProductionLog? ProductionLog { get; set; }
    public Step? WorkInstructionStep { get; set; }
}

public class LogStepValidator : AbstractValidator<List<ProductionLogStep>>
{
    public LogStepValidator()
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
            }).WithMessage("LogSteps must be in ascending order.");
    }
}