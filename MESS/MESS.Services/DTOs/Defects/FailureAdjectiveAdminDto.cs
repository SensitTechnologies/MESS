namespace MESS.Services.DTOs.Defects;

/// <summary>Failure adjective with noun associations for admin UI.</summary>
public class FailureAdjectiveAdminDto
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Associated noun ids.</summary>
    public List<int> NounIds { get; set; } = [];
}
