using FluentValidation;

namespace MESS.Services.DTOs.Rework;

/// <summary>
/// Structural validation for <see cref="ReworkUpdateRequest"/>. Database consistency rules
/// (step/noun/product belong to the instruction, operator active, etc.) are enforced in <c>ReworkService</c>.
/// </summary>
public class ReworkUpdateRequestValidator : AbstractValidator<ReworkUpdateRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReworkUpdateRequestValidator"/> class.
    /// </summary>
    public ReworkUpdateRequestValidator()
    {
        RuleFor(x => x.AttemptId).GreaterThan(0);
        RuleFor(x => x.Original).NotNull();
        RuleFor(x => x.Updated).NotNull();

        RuleFor(x => x.Updated.ProductId)
            .GreaterThan(0).WithMessage("Product is required.")
            .When(x => x.Updated != null);

        RuleFor(x => x.Updated.WorkInstructionId)
            .GreaterThan(0).WithMessage("Instruction is required.")
            .When(x => x.Updated != null);

        RuleFor(x => x.Updated.WorkInstructionStepId)
            .GreaterThan(0).WithMessage("Step is required.")
            .When(x => x.Updated != null);

        RuleFor(x => x.Updated.OperatorId)
            .NotEmpty().WithMessage("Operator is required.")
            .When(x => x.Updated != null);

        RuleFor(x => x.Updated.FailureAdjectiveId)
            .Null().WithMessage("An adjective requires a noun.")
            .When(x => x.Updated is { FailureNounId: null });
    }
}
