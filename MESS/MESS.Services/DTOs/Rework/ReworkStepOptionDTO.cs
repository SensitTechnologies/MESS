namespace MESS.Services.DTOs.Rework;

/// <summary>
/// A Step node of a work instruction, offered in the Rework dialog.
/// </summary>
public sealed class ReworkStepOptionDTO
{
    /// <summary>Step node id.</summary>
    public int Id { get; set; }

    /// <summary>Step name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Position within the instruction.</summary>
    public int Position { get; set; }
}
