namespace MESS.Services.DTOs.Defects;

/// <summary>Minimal id/name row for comboboxes (noun or adjective).</summary>
public class DefectCodeOptionDto
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;
}
