using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog;
using MESS.Services.BrowserCacheManager;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MESS.Tests.UI_Testing.ProductionLog;
using Data.Models;

public class WorkInstructionStepListTests : TestContext
{
    [Fact]
    public void WorkInstructionStepListComponentRendersWithNoSteps()
    {
        // Arrange
        var activeWorkInstruction = new WorkInstruction { Title = "WorkInstruction 1", Steps = new List<Step>() };
        var productionLog = new ProductionLog { LogSteps = new List<ProductionLogStep>() };

        // Act
        var cut = RenderComponent<WorkInstructionStepList>(parameters => parameters
            .Add(p => p.ActiveWorkInstruction, activeWorkInstruction)
            .Add(p => p.ProductionLog, productionLog));

        // Assert
        var listGroup = cut.Find("ul.list-group");
        Assert.Empty(listGroup.Children);
    }

    [Fact]
    public void WorkInstructionStepListComponentRendersWithSteps()
    {
        // Arrange
        var steps = new List<Step>
        {
            new Step { Id = 1, Name = "Step 1" },
            new Step { Id = 2, Name = "Step 2" }
        };
        var activeWorkInstruction = new WorkInstruction { Title = "WorkInstruction1", Steps = steps };
        var productionLog = new ProductionLog { LogSteps = new List<ProductionLogStep>() };

        var mockLocalCacheManager = new Mock<ILocalCacheManager>();
        Services.AddSingleton(mockLocalCacheManager.Object);

        // Act
        var cut = RenderComponent<WorkInstructionStepList>(parameters => parameters
            .Add(p => p.ActiveWorkInstruction, activeWorkInstruction)
            .Add(p => p.ProductionLog, productionLog));

        // Assert
        var listItems = cut.FindAll("li.list-group-item");
        Assert.Equal(2, listItems.Count);
        Assert.Contains("Step 1", listItems[0].TextContent);
        Assert.Contains("Step 2", listItems[1].TextContent);
    }

    [Fact]
    public void WorkInstructionStepListComponentHandlesStepCompletion()
    {
        // Arrange
        var steps = new List<Step>
        {
            new Step { Id = 1, Name = "Step 1" }
        };
        var activeWorkInstruction = new WorkInstruction { Title = "WorkInstruction1", Steps = steps };
        var productionLog = new ProductionLog { LogSteps = new List<ProductionLogStep>() };
        var stepCompleted = false;
        
        var mockLocalCacheManager = new Mock<ILocalCacheManager>();
        Services.AddSingleton(mockLocalCacheManager.Object);

        var cut = RenderComponent<WorkInstructionStepList>(parameters => parameters
            .Add(p => p.ActiveWorkInstruction, activeWorkInstruction)
            .Add(p => p.ProductionLog, productionLog)
            .Add(p => p.OnStepCompleted, EventCallback.Factory.Create<(ProductionLogStep, bool?)>(this, _ => stepCompleted = true)));

        // Act
        var radioButton = cut.Find("input[type='radio']");
        radioButton.Click();

        // Assert
        Assert.True(stepCompleted);
    }
    
    [Fact]
    public void WorkInstructionStepListComponentHandlesMultipleStepCompletions()
    {
        // Arrange
        var steps = new List<Step>
        {
            new Step { Id = 1, Name = "Step 1" },
            new Step { Id = 2, Name = "Step 2" }
        };
        var activeWorkInstruction = new WorkInstruction { Title = "WorkInstruction1", Steps = steps };
        var productionLog = new ProductionLog { LogSteps = new List<ProductionLogStep>() };
        var completedSteps = new List<int>();
        
        var mockLocalCacheManager = new Mock<ILocalCacheManager>();
        Services.AddSingleton(mockLocalCacheManager.Object);

        var cut = RenderComponent<WorkInstructionStepList>(parameters => parameters
            .Add(p => p.ActiveWorkInstruction, activeWorkInstruction)
            .Add(p => p.ProductionLog, productionLog)
            .Add(p => p.OnStepCompleted, EventCallback.Factory.Create<(ProductionLogStep, bool?)>(this, args => completedSteps.Add(args.Item1.WorkInstructionStepId))));
        
        // Act
        var radioButtons = cut.FindAll("input[type='radio']");
        
        // 0 = Failure Button 1 = Success Button
        // 1 = Failure Button 2 = Success Button
        radioButtons[0].Click();
        radioButtons[2].Click();

        // Assert
        Assert.Equal(2, completedSteps.Count);
        Assert.Contains(1, completedSteps);
        Assert.Contains(2, completedSteps);
    }
    
    [Fact]
    public void WorkInstructionStepListComponentHandlesCompletionWithExistingLogSteps()
    {
        // Arrange
        var steps = new List<Step>
        {
            new Step { Id = 1, Name = "Step 1" },
            new Step { Id = 2, Name = "Step 2" }
        };
        var activeWorkInstruction = new WorkInstruction { Title = "WorkInstruction1", Steps = steps };
        var productionLog = new ProductionLog
        {
            LogSteps = new List<ProductionLogStep>
            {
                new ProductionLogStep { WorkInstructionStepId = 1, ProductionLogId = 1 }
            }
        };
        var stepCompleted = false;
        
        var mockLocalCacheManager = new Mock<ILocalCacheManager>();
        Services.AddSingleton(mockLocalCacheManager.Object);

        var cut = RenderComponent<WorkInstructionStepList>(parameters => parameters
            .Add(p => p.ActiveWorkInstruction, activeWorkInstruction)
            .Add(p => p.ProductionLog, productionLog)
            .Add(p => p.OnStepCompleted, EventCallback.Factory.Create<(ProductionLogStep, bool?)>(this, _ => stepCompleted = true)));

        // Act
        var radioButton = cut.Find("input[type='radio']");
        radioButton.Click();

        // Assert
        Assert.True(stepCompleted);
    }
}