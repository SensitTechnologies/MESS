using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog;
using MESS.Data.Models;
using Microsoft.AspNetCore.Components;

namespace MESS.Tests.UI_Testing.ProductionLog;

public class WorkStationSelectTests : TestContext
{
    [Fact]
    public void WorkStationSelectComponentRendersWith1Selection()
    {
        // Act
        var cut = RenderComponent<WorkStationSelect>(parameters => parameters
            .Add(p => p.WorkStations, new List<WorkStation>()));

        // Assert
        var selectElement = cut.Find("select#workstation-select");
        Assert.Equal(1, selectElement.Children.Length);
    }
    
    [Fact]
    public void WorkStationSelectComponentRendersWithWorkStations()
    {
        // Arrange
        var workStations = new List<WorkStation>
        {
            new WorkStation { Id = 1, Name = "Station 1" },
            new WorkStation { Id = 2, Name = "Station 2" }
        };

        // Act
        var cut = RenderComponent<WorkStationSelect>(parameters => parameters
            .Add(p => p.WorkStations, workStations));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(3, options.Count); // Including the default "Select Station" option
        Assert.Equal("Station 1", options[1].TextContent);
        Assert.Equal("Station 2", options[2].TextContent);
    }

    [Fact]
    public void WorkStationSelectComponentFiresOnWorkStationSelectedEvent()
    {
        // Arrange
        var selectedWorkStationId = 0;
        var cut = RenderComponent<WorkStationSelect>(parameters => parameters
            .Add(p => p.OnWorkStationSelected, EventCallback.Factory.Create<int>(this, id => selectedWorkStationId = id))
            .Add(p => p.WorkStations, new List<WorkStation>
            {
                new WorkStation { Id = 1, Name = "Station 1" },
                new WorkStation { Id = 2, Name = "Station 2" }
            }));

        // Act
        var selectElement = cut.Find("select#workstation-select");
        selectElement.Change("2");

        // Assert
        Assert.Equal(2, selectedWorkStationId);
    }

    [Fact]
    public void WorkStationSelectComponentHandlesEmptyWorkStationName()
    {
        // Arrange
        var workStations = new List<WorkStation>
        {
            new WorkStation { Id = 1, Name = "" }
        };

        // Act
        var cut = RenderComponent<WorkStationSelect>(parameters => parameters
            .Add(p => p.WorkStations, workStations));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(2, options.Count); // Including the default "Select Station" option
        Assert.Equal("", options[1].TextContent);
    }

    [Fact]
    public void WorkStationSelectComponentHandlesLongWorkStationName()
    {
        // Arrange
        var longName = new string('A', 1000);
        var workStations = new List<WorkStation>
        {
            new WorkStation { Id = 1, Name = longName }
        };

        // Act
        var cut = RenderComponent<WorkStationSelect>(parameters => parameters
            .Add(p => p.WorkStations, workStations));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(2, options.Count); // Including the default "Select Station" option
        Assert.Equal(longName, options[1].TextContent);
    }

    [Fact]
    public void WorkStationSelectComponentHandlesDuplicateWorkStationIds()
    {
        // Arrange
        var workStations = new List<WorkStation>
        {
            new WorkStation { Id = 1, Name = "Station 1" },
            new WorkStation { Id = 1, Name = "Station 2" }
        };

        // Act
        var cut = RenderComponent<WorkStationSelect>(parameters => parameters
            .Add(p => p.WorkStations, workStations));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(3, options.Count); // Including the default "Select Station" option
        Assert.Equal("Station 1", options[1].TextContent);
        Assert.Equal("Station 2", options[2].TextContent);
    }

    [Fact]
    public void WorkStationSelectComponentHandlesLargeNumberOfWorkStations()
    {
        // Arrange
        var workStations = Enumerable.Range(1, 1000).Select(i => new WorkStation
        {
            Id = i,
            Name = $"Station {i}"
        }).ToList();

        // Act
        var cut = RenderComponent<WorkStationSelect>(parameters => parameters
            .Add(p => p.WorkStations, workStations));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(1001, options.Count); // Including the default "Select Station" option
        Assert.Equal("Station 1", options[1].TextContent);
        Assert.Equal("Station 1000", options[1000].TextContent);
    }
}