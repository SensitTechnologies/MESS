using Microsoft.Playwright.Xunit;

namespace MESS.Tests.UI_Testing.ProductionLog.EndToEnd;

public class ProductionLogCreationE2ETests : PageTest
{
    [Fact]
    public async Task Login_PersistAuthState_Successfully()
    {
        var page = await Page.Context.NewPageAsync();

        await page.GotoAsync("https://localhost:7152");
        await page.GetByText("").SelectOptionAsync("TechnicianUser");
        await Page.Context.StorageStateAsync(new()
        {
            Path = "../../../playwright/.auth/state.json"
        });
    }
    
    [Fact]
    public async Task CreateProductionLog_AsOperator_SuccessfullyCreatesProductionLog()
    {
        await Page.GotoAsync("https://localhost:7152");
    }
}