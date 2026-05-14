namespace MESS.Services.DTOs.Defects;

/// <summary>Failure noun with adjective associations for admin UI.</summary>
public class FailureNounAdminDto
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Associated adjective ids.</summary>
    public List<int> AdjectiveIds { get; set; } = [];
}
