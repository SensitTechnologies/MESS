namespace MESS.Services.DTOs.Rework;

/// <summary>
/// The editable fields of a Rework attempt. Used both as the snapshot loaded into the
/// dialog and as the values the user wants to save; record equality drives change detection.
/// </summary>
public sealed record ReworkEditValues
{
    /// <summary>Attempt outcome; <c>null</c> when not recorded.</summary>
    public bool? Success { get; set; }

    /// <summary>Failure noun id.</summary>
    public int? FailureNounId { get; set; }

    /// <summary>Failure adjective id.</summary>
    public int? FailureAdjectiveId { get; set; }

    /// <summary>Product of the owning log.</summary>
    public int ProductId { get; set; }

    /// <summary>Work instruction of the owning log.</summary>
    public int WorkInstructionId { get; set; }

    /// <summary>Operator (user id) of the owning log.</summary>
    public string? OperatorId { get; set; }

    /// <summary>Work instruction step (Step node id) the attempt belongs to.</summary>
    public int WorkInstructionStepId { get; set; }

    /// <summary>True when any field stored on the production log (product, instruction, operator) differs.</summary>
    public bool ChangesLogFrom(ReworkEditValues other)
        => ProductId != other.ProductId
           || WorkInstructionId != other.WorkInstructionId
           || !string.Equals(OperatorId, other.OperatorId, StringComparison.Ordinal);
}
