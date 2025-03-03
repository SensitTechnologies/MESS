using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog;
using MESS.Blazor.Components.Utility;
using MESS.Data.Models;
using Microsoft.AspNetCore.Components;

namespace MESS.Tests.UI_Testing.Utility;

public class TimePickerTests : TestContext
{
    [Fact]
    public void TimePickerComponentRendersWithDefaultValues()
    {
        // Act
        var cut = RenderComponent<TimePicker>(parameters => parameters
            .Add(p => p.Time, DateTimeOffset.Now));

        // Assert
        var inputElement = cut.Find("input[type='time']");
        Assert.NotNull(inputElement);
    }

    [Fact]
    public void TimePickerComponentRendersWithMinAndMaxValues()
    {
        // Arrange
        var minTime = DateTimeOffset.Now.AddHours(-1);
        var maxTime = DateTimeOffset.Now.AddHours(1);

        // Act
        var cut = RenderComponent<TimePicker>(parameters => parameters
            .Add(p => p.Time, DateTimeOffset.Now)
            .Add(p => p.Min, minTime)
            .Add(p => p.Max, maxTime));

        // Assert
        var inputElement = cut.Find("input[type='time']");
        Assert.Equal(minTime.LocalDateTime.ToString("HH:mm:ss"), inputElement.GetAttribute("min"));
        Assert.Equal(maxTime.LocalDateTime.ToString("HH:mm:ss"), inputElement.GetAttribute("max"));
    }

    [Fact]
    public void TimePickerComponentFiresTimeChangedEvent()
    {
        // Arrange
        var newTime = DateTimeOffset.Now.AddHours(1);
        var timeChanged = false;
        var cut = RenderComponent<TimePicker>(parameters => parameters
            .Add(p => p.Time, DateTimeOffset.Now)
            .Add(p => p.TimeChanged, EventCallback.Factory.Create<DateTimeOffset>(this, _ => timeChanged = true)));

        // Act
        var inputElement = cut.Find("input[type='time']");
        inputElement.Change(newTime.LocalDateTime.ToString("HH:mm:ss"));

        // Assert
        Assert.True(timeChanged);
    }

    [Fact]
    public void TimePickerComponentDoesNotFireTimeChangedEventWhenOutOfRange()
    {
        // Arrange
        var minTime = DateTimeOffset.Now.AddHours(-1);
        var maxTime = DateTimeOffset.Now.AddHours(1);
        var newTime = DateTimeOffset.Now.AddHours(2);
        var timeChanged = false;
        var cut = RenderComponent<TimePicker>(parameters => parameters
            .Add(p => p.Time, DateTimeOffset.Now)
            .Add(p => p.Min, minTime)
            .Add(p => p.Max, maxTime)
            .Add(p => p.TimeChanged, EventCallback.Factory.Create<DateTimeOffset>(this, _ => timeChanged = true)));

        // Act
        var inputElement = cut.Find("input[type='time']");
        inputElement.Change(newTime.LocalDateTime.ToString("HH:mm:ss"));

        // Assert
        Assert.False(timeChanged);
    }
}