namespace MESS.Blazor.Components.Pages.AdminLogViewer;

/// <summary>
/// Parses user-typed date filter values on the Log Archive page into UTC-anchored
/// <see cref="DateTimeOffset"/> ranges. Extracted to a static class so the logic can
/// be exercised by unit tests without instantiating the Blazor component.
/// </summary>
/// <remarks>
/// Both parsers stamp <see cref="TimeSpan.Zero"/> as the offset so the resulting
/// <see cref="DateTimeOffset"/> binds cleanly to PostgreSQL <c>timestamp with time zone</c>
/// via Npgsql (which rejects non-zero offsets).
/// </remarks>
public static class LogArchiveFilterParser
{
    /// <summary>
    /// Parses a "from" date filter value. Returns the start of the given UTC day
    /// (midnight, offset 0), or <c>null</c> if the input is empty or unparseable.
    /// </summary>
    /// <remarks>
    /// Built from Y/M/D components so the result is independent of <see cref="DateTime.Kind"/>.
    /// (The 2-arg <see cref="DateTimeOffset"/> ctor throws when a <c>Local</c>-kind DateTime
    /// is paired with a mismatched offset — a subtle footgun on non-UTC hosts.)
    /// </remarks>
    public static DateTimeOffset? ParseDateStart(string? value)
        => DateTime.TryParse(value, out var date)
            ? new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero)
            : null;

    /// <summary>
    /// Parses a "to" date filter value. Returns the last tick of the given UTC day
    /// (23:59:59.9999999, offset 0), or <c>null</c> if the input is empty or unparseable.
    /// </summary>
    public static DateTimeOffset? ParseDateEnd(string? value)
        => DateTime.TryParse(value, out var date)
            ? new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero)
                .AddDays(1).AddTicks(-1)
            : null;
}
