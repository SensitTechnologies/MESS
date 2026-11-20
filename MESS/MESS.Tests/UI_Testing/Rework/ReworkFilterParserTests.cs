using MESS.Blazor.Components.Pages.Phoebe.Rework;

namespace MESS.Tests.UI_Testing.Rework;

public class ReworkFilterParserTests
{
    [Theory]
    [InlineData("1234", 1234)]
    [InlineData(" 42 ", 42)]
    [InlineData("abc", null)]
    [InlineData("12a", null)]
    [InlineData("-5", null)]
    [InlineData("0", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void ParseId_IgnoresNonNumericInput(string? input, int? expected)
    {
        Assert.Equal(expected, ReworkFilterParser.ParseId(input));
    }

    [Fact]
    public void FromUri_DeepLinkId_PrefillsIdFilter()
    {
        var state = ReworkFilterParser.FromUri("https://host/rework?id=1234");

        Assert.Equal("1234", state.Filters.Id);
        Assert.Equal(1, state.Page);
        Assert.Equal(ReworkFilterParser.DefaultPageSize, state.PageSize);
        Assert.Equal("submitTime", state.SortBy);
        Assert.Equal("desc", state.SortDir);
    }

    [Fact]
    public void FromUri_InvalidPageSizeAndSortDir_FallBackToDefaults()
    {
        var state = ReworkFilterParser.FromUri("/rework?pageSize=7&sortDir=sideways");

        Assert.Equal(50, state.PageSize);
        Assert.Equal("desc", state.SortDir);
    }

    [Fact]
    public void ToUri_RoundTripsFiltersAndOmitsDefaults()
    {
        var state = new ReworkPageState
        {
            Page = 2,
            Filters = new ReworkFilters { Id = "77", Noun = "Housing & Case", Success = ReworkFilterParser.FailureValue }
        };

        var uri = ReworkFilterParser.ToUri(state);
        var parsed = ReworkFilterParser.FromUri(uri);

        Assert.StartsWith("/rework?", uri);
        Assert.DoesNotContain("pageSize", uri);
        Assert.DoesNotContain("sortBy", uri);
        Assert.Equal(2, parsed.Page);
        Assert.Equal("77", parsed.Filters.Id);
        Assert.Equal("Housing & Case", parsed.Filters.Noun);
        Assert.Equal(ReworkFilterParser.FailureValue, parsed.Filters.Success);
    }

    [Fact]
    public void ToUri_NoFilters_IsBareRoute()
    {
        Assert.Equal("/rework", ReworkFilterParser.ToUri(new ReworkPageState()));
    }

    [Theory]
    [InlineData(ReworkFilterParser.SuccessValue, true, false)]
    [InlineData(ReworkFilterParser.FailureValue, false, false)]
    [InlineData(ReworkFilterParser.NotRecordedValue, null, true)]
    [InlineData(null, null, false)]
    public void ToQuery_MapsSuccessFilter(string? value, bool? expectedSuccess, bool expectedNotRecorded)
    {
        var query = ReworkFilterParser.ToQuery(new ReworkPageState { Filters = new ReworkFilters { Success = value } });

        Assert.Equal(expectedSuccess, query.FilterSuccess);
        Assert.Equal(expectedNotRecorded, query.FilterSuccessNotRecorded);
    }

    [Fact]
    public void ToQuery_SubmitDates_AreUtcCalendarDayBounds()
    {
        var query = ReworkFilterParser.ToQuery(new ReworkPageState
        {
            Filters = new ReworkFilters { SubmitFrom = "2026-11-01", SubmitTo = "2026-11-20" }
        });

        Assert.Equal(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero), query.FilterSubmitFrom);
        Assert.Equal(new DateTimeOffset(2026, 11, 21, 0, 0, 0, TimeSpan.Zero).AddTicks(-1), query.FilterSubmitTo);
        Assert.Equal(TimeSpan.Zero, query.FilterSubmitFrom!.Value.Offset);
        Assert.Equal(TimeSpan.Zero, query.FilterSubmitTo!.Value.Offset);
    }

    [Fact]
    public void ToQuery_NonNumericId_IsIgnored()
    {
        var query = ReworkFilterParser.ToQuery(new ReworkPageState { Filters = new ReworkFilters { Id = "RT-12" } });

        Assert.Null(query.FilterId);
    }

    [Fact]
    public void Filters_HasAnyAndClear()
    {
        var filters = new ReworkFilters();
        Assert.False(filters.HasAny);

        filters.Step = "weld";
        Assert.True(filters.HasAny);

        filters.Clear();
        Assert.False(filters.HasAny);
    }
}
