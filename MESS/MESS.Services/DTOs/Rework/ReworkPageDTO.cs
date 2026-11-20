namespace MESS.Services.DTOs.Rework;

/// <summary>
/// Paged Rework response.
/// </summary>
public sealed class ReworkPageDTO
{
    /// <summary>Rows for the requested page.</summary>
    public List<ReworkRowDTO> Data { get; set; } = [];

    /// <summary>Total rows matching the current query.</summary>
    public int Total { get; set; }

    /// <summary>One-based current page.</summary>
    public int Page { get; set; }

    /// <summary>Rows per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total matching pages.</summary>
    public int TotalPages { get; set; }
}
