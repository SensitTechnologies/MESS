namespace MESS.Services.DTOs.ProductionLogs.Archive;

/// <summary>
/// Paged Log Archives response.
/// </summary>
public sealed class ProductionLogArchivePageDTO
{
    /// <summary>Rows for the requested page.</summary>
    public List<ProductionLogArchiveRowDTO> Data { get; set; } = [];

    /// <summary>Total rows matching the current query.</summary>
    public int Total { get; set; }

    /// <summary>One-based current page.</summary>
    public int Page { get; set; }

    /// <summary>Rows per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total matching pages.</summary>
    public int TotalPages { get; set; }
}
