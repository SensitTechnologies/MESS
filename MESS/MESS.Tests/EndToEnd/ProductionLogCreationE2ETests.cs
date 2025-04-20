using MESS.Tests.UI_Testing.Setup;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace MESS.Tests.UI_Testing.ProductionLog.EndToEnd;

public class ProductionLogCreationE2ETests : PageTest, IClassFixture<AuthFixture>
{
    private readonly AuthFixture _authFixture;

    public ProductionLogCreationE2ETests(AuthFixture authFixture)
    {
        _authFixture = authFixture;
    }
    
    [Fact]
    public async Task CreateProductionLog_AsOperator_SuccessfullyCreatesProductionLog()
    {
        var context = await Browser.NewContextAsync(new()
        {
            StorageStatePath = _authFixture.StorageStatePath,
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();

        await page.GotoAsync("https://localhost:7152/production-log");
        
        // Select Product
        await page.SelectOptionAsync("#product-select", new []{ "G2" });
        
        // Select Work Instruction
        await page.SelectOptionAsync("#workInstruction-select", new []{ new SelectOptionValue { Index = 1} });
        
        // Enter Steps
        await Expect(page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(14);

        var rowLocator = page.GetByRole(AriaRole.Listitem);

        await rowLocator
            .Filter(new LocatorFilterOptions
            {
                Has = page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions
                {
                    Name = "Success"
                })
            })
            .Nth(1)
            .ClickAsync();
        
        await rowLocator
            .Filter(new LocatorFilterOptions
            {
                Has = page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions
                {
                    Name = "Failure"
                })
            })
            .Nth(2)
            .ClickAsync();
        
        // Input text into notes field
        
        // Submit
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions()
        {
            Name = "Submit Log"
        }).ClickAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions()
        {
            Name = "Submit",
            Exact = true
        }).ClickAsync();
        
        // Validate
        
        
        
        await page.CloseAsync();
        await context.CloseAsync();
    }

    [Fact]
    public async Task CreateProductionLog_PartialPartNumberValues_CorrectlyShowsConfirmationAlert()
    {
        var context = await Browser.NewContextAsync(new()
        {
            StorageStatePath = _authFixture.StorageStatePath,
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();
        
        await page.GotoAsync("https://localhost:7152/production-log");
        
        await page.Locator("#product-select").SelectOptionAsync(new[] { "1" });
        await page.SelectOptionAsync("#workInstruction-select", new []{ new SelectOptionValue { Index = 1} });
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Product Serial Number:" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Product Serial Number:" }).FillAsync("000");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Display Board Main Board 884-" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Display Board Main Board 884-" }).FillAsync("000");
        await page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Test display. Failure Success" }).Locator("label").Nth(1).ClickAsync();
        await page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Attach display board to main" }).Locator("label").Nth(1).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Submit Log" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Article)).ToBeVisibleAsync();
    }
}