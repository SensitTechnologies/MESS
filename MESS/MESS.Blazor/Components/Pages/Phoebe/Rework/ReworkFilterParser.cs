using System.Globalization;
using MESS.Blazor.Components.Pages.AdminLogViewer;
using MESS.Services.DTOs.Rework;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace MESS.Blazor.Components.Pages.Phoebe.Rework;

/// <summary>
/// Raw (as typed) filter values of the Rework page. Kept as strings so the inputs echo exactly what the
/// user typed; <see cref="ReworkFilterParser.ToQuery"/> turns them into a typed <see cref="ReworkQuery"/>.
/// </summary>
public sealed class ReworkFilters
{
    /// <summary>Attempt id (Red Tag number).</summary>
    public string? Id { get; set; }

    /// <summary>One of <see cref="ReworkFilterParser.SuccessValue"/>, <see cref="ReworkFilterParser.FailureValue"/>,
    /// <see cref="ReworkFilterParser.NotRecordedValue"/>, or empty for all.</summary>
    public string? Success { get; set; }

    /// <summary>Notes contains.</summary>
    public string? Notes { get; set; }

    /// <summary>Submit date from (yyyy-MM-dd).</summary>
    public string? SubmitFrom { get; set; }

    /// <summary>Submit date to (yyyy-MM-dd).</summary>
    public string? SubmitTo { get; set; }

    /// <summary>Noun contains.</summary>
    public string? Noun { get; set; }

    /// <summary>Adjective contains.</summary>
    public string? Adjective { get; set; }

    /// <summary>Product contains.</summary>
    public string? Product { get; set; }

    /// <summary>Instruction contains.</summary>
    public string? Instruction { get; set; }

    /// <summary>Operator contains.</summary>
    public string? Operator { get; set; }

    /// <summary>Step contains.</summary>
    public string? Step { get; set; }

    /// <summary>True when any filter has a value.</summary>
    public bool HasAny => new[]
    {
        Id, Success, Notes, SubmitFrom, SubmitTo, Noun, Adjective, Product, Instruction, Operator, Step
    }.Any(value => !string.IsNullOrWhiteSpace(value));

    /// <summary>Clears every filter.</summary>
    public void Clear()
    {
        Id = Success = Notes = SubmitFrom = SubmitTo = null;
        Noun = Adjective = Product = Instruction = Operator = Step = null;
    }
}

/// <summary>
/// Page, sort and filter state of the Rework page as reflected in the URL.
/// </summary>
public sealed class ReworkPageState
{
    /// <summary>One-based page.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Rows per page.</summary>
    public int PageSize { get; set; } = ReworkFilterParser.DefaultPageSize;

    /// <summary>Sort column key.</summary>
    public string SortBy { get; set; } = ReworkFilterParser.DefaultSortBy;

    /// <summary>Sort direction.</summary>
    public string SortDir { get; set; } = ReworkFilterParser.DefaultSortDir;

    /// <summary>Column filters.</summary>
    public ReworkFilters Filters { get; set; } = new();
}

/// <summary>
/// Converts Rework page state between the query string, the filter inputs and <see cref="ReworkQuery"/>.
/// Static so it can be unit tested without rendering the page.
/// </summary>
public static class ReworkFilterParser
{
    /// <summary>Route of the Rework page.</summary>
    public const string Route = "/rework";

    /// <summary>Success dropdown value for successful attempts.</summary>
    public const string SuccessValue = "success";

    /// <summary>Success dropdown value for failed attempts.</summary>
    public const string FailureValue = "failure";

    /// <summary>Success dropdown value for attempts with no recorded outcome.</summary>
    public const string NotRecordedValue = "none";

    /// <summary>Default rows per page.</summary>
    public const int DefaultPageSize = 50;

    /// <summary>Default sort column.</summary>
    public const string DefaultSortBy = "submitTime";

    /// <summary>Default sort direction.</summary>
    public const string DefaultSortDir = "desc";

    /// <summary>Allowed page sizes.</summary>
    public static readonly int[] PageSizes = [25, 50, 100, 250];

    /// <summary>
    /// Parses the ID filter. Returns <c>null</c> (no filter) for empty, non-numeric or non-positive input.
    /// </summary>
    public static int? ParseId(string? value)
        => int.TryParse(value?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id >= 1
            ? id
            : null;

    /// <summary>
    /// Builds the service query. Dates become UTC calendar-day bounds with a zero offset, which Npgsql
    /// requires for <c>timestamptz</c> parameters (see bug-log-archive-date-filter.md).
    /// </summary>
    public static ReworkQuery ToQuery(ReworkPageState state)
    {
        var filters = state.Filters;
        return new ReworkQuery
        {
            Page = state.Page,
            PageSize = state.PageSize,
            SortBy = state.SortBy,
            SortDir = state.SortDir,
            FilterId = ParseId(filters.Id),
            FilterSuccess = filters.Success switch
            {
                SuccessValue => true,
                FailureValue => false,
                _ => null
            },
            FilterSuccessNotRecorded = filters.Success == NotRecordedValue,
            FilterNotes = filters.Notes,
            FilterSubmitFrom = LogArchiveFilterParser.ParseDateStart(filters.SubmitFrom),
            FilterSubmitTo = LogArchiveFilterParser.ParseDateEnd(filters.SubmitTo),
            FilterNoun = filters.Noun,
            FilterAdjective = filters.Adjective,
            FilterProduct = filters.Product,
            FilterInstruction = filters.Instruction,
            FilterOperator = filters.Operator,
            FilterStep = filters.Step
        };
    }

    /// <summary>Reads page state from an absolute or relative URI.</summary>
    public static ReworkPageState FromUri(string uri)
    {
        var queryStart = uri.IndexOf('?');
        var query = QueryHelpers.ParseQuery(queryStart >= 0 ? uri[queryStart..] : string.Empty);

        var pageSize = ReadInt(query, "pageSize", DefaultPageSize);
        var sortDir = ReadString(query, "sortDir");

        return new ReworkPageState
        {
            Page = ReadInt(query, "page", 1),
            PageSize = PageSizes.Contains(pageSize) ? pageSize : DefaultPageSize,
            SortBy = ReadString(query, "sortBy") ?? DefaultSortBy,
            SortDir = sortDir is "asc" or "desc" ? sortDir : DefaultSortDir,
            Filters = new ReworkFilters
            {
                Id = ReadString(query, "id"),
                Success = ReadString(query, "success"),
                Notes = ReadString(query, "notes"),
                SubmitFrom = ReadString(query, "submitFrom"),
                SubmitTo = ReadString(query, "submitTo"),
                Noun = ReadString(query, "noun"),
                Adjective = ReadString(query, "adjective"),
                Product = ReadString(query, "product"),
                Instruction = ReadString(query, "instruction"),
                Operator = ReadString(query, "operator"),
                Step = ReadString(query, "step")
            }
        };
    }

    /// <summary>Builds the relative URL for the given state, omitting defaults and empty filters.</summary>
    public static string ToUri(ReworkPageState state)
    {
        var filters = state.Filters;
        var values = new Dictionary<string, string?>
        {
            ["id"] = NullIfEmpty(filters.Id),
            ["success"] = NullIfEmpty(filters.Success),
            ["notes"] = NullIfEmpty(filters.Notes),
            ["submitFrom"] = NullIfEmpty(filters.SubmitFrom),
            ["submitTo"] = NullIfEmpty(filters.SubmitTo),
            ["noun"] = NullIfEmpty(filters.Noun),
            ["adjective"] = NullIfEmpty(filters.Adjective),
            ["product"] = NullIfEmpty(filters.Product),
            ["instruction"] = NullIfEmpty(filters.Instruction),
            ["operator"] = NullIfEmpty(filters.Operator),
            ["step"] = NullIfEmpty(filters.Step),
            ["page"] = state.Page == 1 ? null : state.Page.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = state.PageSize == DefaultPageSize ? null : state.PageSize.ToString(CultureInfo.InvariantCulture),
            ["sortBy"] = state.SortBy == DefaultSortBy ? null : state.SortBy,
            ["sortDir"] = state.SortDir == DefaultSortDir ? null : state.SortDir
        };

        return QueryHelpers.AddQueryString(Route,
            values.Where(kvp => kvp.Value != null).Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value)));
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static int ReadInt(Dictionary<string, StringValues> query, string key, int fallback)
        => query.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? Math.Max(1, parsed) : fallback;

    private static string? ReadString(Dictionary<string, StringValues> query, string key)
        => query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.ToString() : null;
}
