using Bunit;
using MESS.Blazor.Components.Pages.ProductionLog.WorkInstructionNodes;
using MESS.Data.Models;
using MESS.Services.CRUD.Defects;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;
using MESS.Services.UI.ProductionLogEvent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Moq;

namespace MESS.Tests.UI_Testing.ProductionLog;

/// <summary>
/// Pressing Success on a unit that already passed must not record another attempt
/// (repeat presses were stacking duplicate Success rows shown in Log archive → View).
/// </summary>
public class StepNodeListItemAttemptTests : BunitContext
{
    public StepNodeListItemAttemptTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(Mock.Of<IDefectCodeService>());
        Services.AddSingleton(Mock.Of<IProductionLogEventService>());
        Services.AddSingleton(Mock.Of<IToastService>());
    }

    private static Step NewStep() => new()
    {
        Id = 1, Name = "Install Bottom Bracket", Body = "Fit bracket", NodeType = WorkInstructionNodeType.Step
    };

    private static LogStepFormDTO LogStep(params bool[] attempts) => new()
    {
        WorkInstructionStepId = 1,
        Attempts = attempts
            .Select((success, i) => new StepAttemptFormDTO
            {
                IsSuccess = success,
                SubmitTime = DateTimeOffset.UtcNow.AddMinutes(-10 + i)
            })
            .ToList()
    };

    private IRenderedComponent<StepNodeListItem> RenderItem(List<LogStepFormDTO> logSteps) =>
        Render<StepNodeListItem>(p => p
            .Add(c => c.Step, NewStep())
            .Add(c => c.LogSteps, logSteps));

    private static void ClickButton(IRenderedComponent<StepNodeListItem> cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text).Click();

    [Fact]
    public void Success_OnUnattemptedStep_RecordsOneAttempt()
    {
        var logStep = LogStep();
        var cut = RenderItem([logStep]);

        ClickButton(cut, "Success");

        Assert.True(Assert.Single(logStep.Attempts).IsSuccess);
    }

    [Fact]
    public void Success_PressedAgainAfterSuccess_DoesNotAddAttempt()
    {
        var logStep = LogStep();
        var cut = RenderItem([logStep]);

        ClickButton(cut, "Success");
        ClickButton(cut, "Success");
        ClickButton(cut, "Success");

        Assert.Single(logStep.Attempts);
    }

    [Fact]
    public void Success_AfterFailure_RecordsRetry()
    {
        var logStep = LogStep(false);
        var cut = RenderItem([logStep]);

        ClickButton(cut, "Success");

        Assert.Equal(new bool?[] { false, true }, logStep.Attempts.Select(a => a.IsSuccess));
    }

    [Fact]
    public void Failure_AfterSuccess_IsStillRecorded()
    {
        var logStep = LogStep(true);
        var cut = RenderItem([logStep]);

        ClickButton(cut, "Failure");

        Assert.Equal(new bool?[] { true, false }, logStep.Attempts.Select(a => a.IsSuccess));
    }

    [Fact]
    public void PassAll_OnlyAddsSuccessToUnitsNotAlreadyPassed()
    {
        var passed = LogStep(true);
        var failed = LogStep(false);
        var untouched = LogStep();
        var cut = RenderItem([passed, failed, untouched]);

        ClickButton(cut, "Pass All");
        ClickButton(cut, "Pass All");

        Assert.Single(passed.Attempts);
        Assert.Equal(new bool?[] { false, true }, failed.Attempts.Select(a => a.IsSuccess));
        Assert.True(Assert.Single(untouched.Attempts).IsSuccess);
        Assert.NotSame(failed.Attempts[^1], untouched.Attempts[0]);
    }
}
