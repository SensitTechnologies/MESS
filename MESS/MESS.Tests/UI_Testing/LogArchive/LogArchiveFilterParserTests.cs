using MESS.Blazor.Components.Pages.AdminLogViewer;

namespace MESS.Tests.UI_Testing.LogArchive;

/// <summary>
/// Verifies that <see cref="LogArchiveFilterParser"/> produces UTC-anchored
/// <see cref="DateTimeOffset"/> values so that filter parameters bind cleanly
/// to PostgreSQL <c>timestamp with time zone</c> via Npgsql.
/// </summary>
public class LogArchiveFilterParserTests
{
    [Fact]
    public void ParseDateStart_ValidDate_ReturnsUtcOffset()
    {
        var result = LogArchiveFilterParser.ParseDateStart("2026-06-01");

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.Zero, result!.Value.Offset);
    }

    [Fact]
    public void ParseDateStart_ValidDate_ReturnsMidnightUtc()
    {
        var result = LogArchiveFilterParser.ParseDateStart("2026-06-01");

        Assert.NotNull(result);
        Assert.Equal(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero), result);
    }

    [Fact]
    public void ParseDateStart_Null_ReturnsNull()
    {
        Assert.Null(LogArchiveFilterParser.ParseDateStart(null));
    }

    [Fact]
    public void ParseDateStart_InvalidDate_ReturnsNull()
    {
        Assert.Null(LogArchiveFilterParser.ParseDateStart("not a date"));
    }

    [Fact]
    public void ParseDateEnd_ValidDate_ReturnsEndOfDayUtc()
    {
        var result = LogArchiveFilterParser.ParseDateEnd("2026-06-01");

        Assert.NotNull(result);
        Assert.Equal(TimeSpan.Zero, result!.Value.Offset);
        // Last tick of 2026-06-01 (23:59:59.9999999 UTC).
        var expected = new DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero).AddTicks(-1);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Regression guard: the bug was that <c>new DateTimeOffset(date.Date)</c> stamped the
    /// host's local offset. The parsers must always return UTC (offset 0), regardless of
    /// the process time zone. We can't reliably mutate <c>TimeZoneInfo.Local</c> mid-test,
    /// but we can construct a synthetic non-UTC local <see cref="DateTime"/> and confirm
    /// the parser still normalizes to UTC-anchored 0 offset.
    /// </summary>
    [Fact]
    public void Parsers_AlwaysReturnUtcOffset_RegardlessOfInputKind()
    {
        // Any parseable string (with or without a timezone hint) must land at offset 0.
        foreach (var input in new[]
                 {
                     "2026-06-01",
                     "2026-06-01T12:34:56",
                     "2026-06-01T12:34:56-05:00",
                     "06/01/2026",
                 })
        {
            var start = LogArchiveFilterParser.ParseDateStart(input);
            var end = LogArchiveFilterParser.ParseDateEnd(input);

            Assert.NotNull(start);
            Assert.NotNull(end);
            Assert.Equal(TimeSpan.Zero, start!.Value.Offset);
            Assert.Equal(TimeSpan.Zero, end!.Value.Offset);
        }
    }
}
