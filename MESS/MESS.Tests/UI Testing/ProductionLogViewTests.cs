using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog;

namespace MESS.Tests.UI_Testing;

public class ProductionLogViewTests : TestContext
{
    [Fact]
    public void HelloWorldComponentRendersCorrectly()
    {
        // Act
        var cut = RenderComponent<ProductionLogRadioButton>();

        // Assert
        cut.MarkupMatches("<h1>Hello world from Blazor</h1>");
    }
}