using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog;
using MESS.Data.Models;
using MESS.Services.UI.ProductionLogEvent;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MESS.Tests.UI_Testing.ProductionLog;
public class ProductSelectTests : TestContext
{
    private readonly Mock<IProductionLogEventService> _mockProductionLogEventService;
    public ProductSelectTests()
    {
        _mockProductionLogEventService = new Mock<IProductionLogEventService>();
        Services.AddSingleton(_mockProductionLogEventService.Object);
    }
    
    [Fact]
    public void ProductSelectComponentFiresOnProductSelectedEvent()
    {
        // Arrange
        var selectedProductId = 0;
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.OnProductSelected, EventCallback.Factory.Create<int>(this, id => selectedProductId = id))
            .Add(p => p.Products, new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", WorkInstructions = new List<WorkInstruction>
                    {
                        new WorkInstruction
                        {
                            Title = "Work Instruction 1",
                            Nodes = new List<WorkInstructionNode>
                            {
                                new Step
                                {
                                    Name = "Step 1",
                                    Body = "Step 1",
                                },
                                new Step
                                {
                                    Name = "Step 2",
                                    Body = "Step 2",
                                },
                                new Step
                                {
                                    Name = "Step 3",
                                    Body = "Step 3",
                                }
                            } 
                        }
                    }
                }
            }));

        // Act
        var selectElement = cut.Find("select#product-select");
        selectElement.Change("2");

        // Assert
        Assert.Equal(2, selectedProductId);
    }
    
    [Fact]
    public void ProductSelectComponentRendersWithOnlyHoverProduct()
    {
        // Act
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.Products, new List<Product>()));

        // Assert
        var selectElement = cut.Find("select#product-select");
        Assert.Equal(1, selectElement.Children.Length);
    }
    
    [Fact]
    public void ProductSelectComponentRendersDisabled()
    {
        // Act
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.Products, new List<Product>())
            .Add(p => p.Disabled, true));

        // Assert
        var selectElement = cut.Find("select#product-select");
        Assert.True(selectElement.HasAttribute("disabled"));
    }
    
    [Fact]
    public void ProductSelectComponentRendersWithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", WorkInstructions = new List<WorkInstruction>(), IsActive = true},
            new Product { Id = 2, Name = "Product 2", WorkInstructions = new List<WorkInstruction>(), IsActive = true}
        };

        // Act
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.Products, products));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(3, options.Count); // Including the default "Select Product" option
        Assert.Equal("Product 1", options[1].TextContent);
        Assert.Equal("Product 2", options[2].TextContent);
    }
    
    [Fact]
    public void ProductSelectComponentHandlesEmptyProductName()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "", WorkInstructions = new List<WorkInstruction>(), IsActive = true}
        };

        // Act
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.Products, products));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(2, options.Count); // Including the default "Select Product" option
        Assert.Equal("", options[1].TextContent);
    }
    
    [Fact]
    public void ProductSelectComponentHandlesLongProductName()
    {
        // Arrange
        var longName = new string('A', 1000);
        var products = new List<Product>
        {
            new Product { Id = 1, Name = longName, WorkInstructions = new List<WorkInstruction>(), IsActive = true}
        };

        // Act
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.Products, products));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(2, options.Count); // Including the default "Select Product" option
        Assert.Equal(longName, options[1].TextContent);
    }
    
    [Fact]
    public void ProductSelectComponentHandlesDuplicateProductIds()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", WorkInstructions = new List<WorkInstruction>(), IsActive = true },
            new Product { Id = 1, Name = "Product 2", WorkInstructions = new List<WorkInstruction>(), IsActive = true }
        };

        // Act
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.Products, products));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(3, options.Count); // Including the default "Select Product" option
        Assert.Equal("Product 1", options[1].TextContent);
        Assert.Equal("Product 2", options[2].TextContent);
    }
    
    [Fact]
    public void ProductSelectComponentHandlesLargeNumberOfProducts()
    {
        // Arrange
        var products = Enumerable.Range(1, 1000).Select(i => new Product
        {
            Id = i,
            Name = $"Product {i}",
            WorkInstructions = new List<WorkInstruction>(),
            IsActive = true
        }).ToList();

        // Act
        var cut = RenderComponent<ProductSelect>(parameters => parameters
            .Add(p => p.Products, products));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(1001, options.Count); // Including the default "Select Product" option
        Assert.Equal("Product 1", options[1].TextContent);
        Assert.Equal("Product 1000", options[1000].TextContent);
    }
}