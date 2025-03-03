using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog;
using MESS.Data.Models;
using Microsoft.AspNetCore.Components;

namespace MESS.Tests.UI_Testing;

public class ProductionLogViewTests : TestContext
{
    
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
                            Steps = new List<Step>
                            {
                                new Step
                                {
                                    Name = "Step 1",
                                },
                                new Step
                                {
                                    Name = "Step 2",
                                },
                                new Step
                                {
                                    Name = "Step 3",
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
            new Product { Id = 1, Name = "Product 1", WorkInstructions = new List<WorkInstruction>() },
            new Product { Id = 2, Name = "Product 2", WorkInstructions = new List<WorkInstruction>() }
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
    

}