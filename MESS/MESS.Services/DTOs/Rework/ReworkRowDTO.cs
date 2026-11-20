namespace MESS.Services.DTOs.Rework;

/// <summary>
/// One production log step attempt as shown on the Rework page.
/// </summary>
public sealed class ReworkRowDTO
{
    /// <summary>Attempt id (printed on the Red Tag).</summary>
    public int Id { get; set; }

    /// <summary>Owning production log id.</summary>
    public int ProductionLogId { get; set; }

    /// <summary>Attempt outcome; <c>null</c> when not recorded.</summary>
    public bool? Success { get; set; }

    /// <summary>Operator notes.</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Time the attempt was submitted (UTC).</summary>
    public DateTimeOffset SubmitTime { get; set; }

    /// <summary>Failure noun (defect location) name.</summary>
    public string NounName { get; set; } = string.Empty;

    /// <summary>Failure adjective (defect type) name.</summary>
    public string AdjectiveName { get; set; } = string.Empty;

    /// <summary>Product name of the owning log.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Work instruction title, including the version when present.</summary>
    public string InstructionName { get; set; } = string.Empty;

    /// <summary>Operator display name ("First Last", falling back to user name).</summary>
    public string OperatorName { get; set; } = string.Empty;

    /// <summary>Work instruction step name.</summary>
    public string StepName { get; set; } = string.Empty;
}
