using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog;
using MESS.Data.Models;

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
        var selectElement = cut.Find("select#product-select");
        Assert.Equal(1, selectElement.Children.Length);
    }
}