namespace MESS.Services.DTOs.Rework;

/// <summary>
/// Server-side query parameters for the Rework page (production log step attempts).
/// </summary>
public sealed class ReworkQuery
{
    /// <summary>One-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of rows per page (25, 50, 100 or 250).</summary>
    public int PageSize { get; set; } = 50;

    /// <summary>Sort column key.</summary>
    public string? SortBy { get; set; } = "submitTime";

    /// <summary>Sort direction, asc or desc.</summary>
    public string? SortDir { get; set; } = "desc";

    /// <summary>Exact attempt id (the number printed on the Red Tag).</summary>
    public int? FilterId { get; set; }

    /// <summary>Success filter; <c>null</c> means all (unless <see cref="FilterSuccessNotRecorded"/> is set).</summary>
    public bool? FilterSuccess { get; set; }

    /// <summary>When true, only attempts whose Success is <c>null</c> are returned.</summary>
    public bool FilterSuccessNotRecorded { get; set; }

    /// <summary>Notes contains filter.</summary>
    public string? FilterNotes { get; set; }

    /// <summary>Submit time range start. Offset must be <see cref="TimeSpan.Zero"/>.</summary>
    public DateTimeOffset? FilterSubmitFrom { get; set; }

    /// <summary>Submit time range end. Offset must be <see cref="TimeSpan.Zero"/>.</summary>
    public DateTimeOffset? FilterSubmitTo { get; set; }

    /// <summary>Failure noun contains filter.</summary>
    public string? FilterNoun { get; set; }

    /// <summary>Failure adjective contains filter.</summary>
    public string? FilterAdjective { get; set; }

    /// <summary>Product contains filter.</summary>
    public string? FilterProduct { get; set; }

    /// <summary>Work instruction (title and version) contains filter.</summary>
    public string? FilterInstruction { get; set; }

    /// <summary>Operator display name contains filter.</summary>
    public string? FilterOperator { get; set; }

    /// <summary>Step name contains filter.</summary>
    public string? FilterStep { get; set; }
}
