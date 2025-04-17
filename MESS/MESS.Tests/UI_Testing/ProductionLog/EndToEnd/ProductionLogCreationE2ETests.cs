using Microsoft.Playwright.Xunit;

namespace MESS.Tests.UI_Testing.ProductionLog.EndToEnd;

public class ProductionLogCreationE2ETests : PageTest
{
    [Fact]
    public async Task CreateProductionLog_AsOperator_SuccessfullyCreatesProductionLog()
    {
        await Page.GotoAsync("https://localhost:7152");
    }
}