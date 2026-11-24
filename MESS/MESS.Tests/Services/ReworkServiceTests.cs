using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.CRUD.Rework;
using MESS.Services.DTOs.Rework;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MESS.Tests.Services;

/// <summary>
/// ReworkService against a real (SQLite in-memory) ApplicationContext.
/// SQLite cannot ORDER BY or compare DateTimeOffset, so page queries here sort by id and
/// the UTC date filter is covered by ReworkFilterParserTests.
/// </summary>
public class ReworkServiceTests : IDisposable
{
    private const string Technician = "tech";

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationContext> _options;
    private readonly SeedData _seed;

    public ReworkServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<ApplicationContext>().UseSqlite(_connection).Options;

        using var context = new ApplicationContext(_options);
        context.Database.EnsureCreated();
        _seed = Seed(context);
    }

    public void Dispose() => _connection.Dispose();

    private ReworkService CreateService()
    {
        var factory = new Mock<IDbContextFactory<ApplicationContext>>();
        factory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ApplicationContext(_options));
        return new ReworkService(factory.Object);
    }

    private ApplicationContext NewContext() => new(_options);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed record SeedData(
        int ProductOne, int ProductTwo,
        int WiA, int WiB,
        int StepA1, int StepA2, int StepB1,
        int NounOne, int NounTwo, int NounB,
        int AdjOne, int AdjTwo,
        int LogMulti, int LogSingle,
        int AttemptFailed, int AttemptSuccess, int AttemptSingle, int AttemptNotRecorded);

    private static SeedData Seed(ApplicationContext context)
    {
        var techRole = new IdentityRole("Technician") { NormalizedName = "TECHNICIAN" };
        var operatorRole = new IdentityRole("Operator") { NormalizedName = "OPERATOR" };
        context.Roles.AddRange(techRole, operatorRole);
        context.Users.AddRange(
            new ApplicationUser { Id = "tech-id", UserName = Technician, NormalizedUserName = "TECH", IsActive = true },
            new ApplicationUser { Id = "op1", UserName = "op1", NormalizedUserName = "OP1", FirstName = "Ann", LastName = "Op", IsActive = true },
            new ApplicationUser { Id = "op2", UserName = "op2", NormalizedUserName = "OP2", FirstName = "Bob", LastName = "Op", IsActive = true },
            new ApplicationUser { Id = "op-old", UserName = "old", NormalizedUserName = "OLD", IsActive = false });
        context.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = "tech-id", RoleId = techRole.Id },
            new IdentityUserRole<string> { UserId = "op1", RoleId = operatorRole.Id });

        var productOne = new Product { PartDefinition = new PartDefinition { Name = "Prod One", Number = "P-1" }, IsActive = true };
        var productTwo = new Product { PartDefinition = new PartDefinition { Name = "Prod Two", Number = "P-2" }, IsActive = true };

        var stepA1 = new Step { Name = "Step 1", Body = "", Position = 0, NodeType = WorkInstructionNodeType.Step };
        var stepA2 = new Step { Name = "Step 2", Body = "", Position = 1, NodeType = WorkInstructionNodeType.Step };
        var stepB1 = new Step { Name = "Other Step", Body = "", Position = 0, NodeType = WorkInstructionNodeType.Step };

        var adjOne = new FailureAdjective { Name = "Cracked" };
        var adjTwo = new FailureAdjective { Name = "Loose" };
        var nounOne = new FailureNoun { Name = "Housing", Adjectives = [adjOne] };
        var nounTwo = new FailureNoun { Name = "Screw", Adjectives = [adjTwo] };
        var nounB = new FailureNoun { Name = "Cable", Adjectives = [adjOne] };

        var wiA = new WorkInstruction
        {
            Title = "WI-A", Version = "1.0", Products = [productOne], Nodes = [stepA1, stepA2], FailureNouns = [nounOne, nounTwo]
        };
        var wiB = new WorkInstruction
        {
            Title = "WI-B", Products = [productOne, productTwo], Nodes = [stepB1], FailureNouns = [nounB]
        };
        context.WorkInstructions.AddRange(wiA, wiB);
        context.SaveChanges();

        var now = DateTimeOffset.UtcNow;
        var failed = new ProductionLogStepAttempt
        {
            Success = false, Notes = "Cracked housing", SubmitTime = now, FailureNounId = nounOne.Id, FailureAdjectiveId = adjOne.Id
        };
        var success = new ProductionLogStepAttempt { Success = true, Notes = "ok", SubmitTime = now };
        var notRecorded = new ProductionLogStepAttempt { Success = null, SubmitTime = now };
        var logMulti = new ProductionLog
        {
            ProductId = productOne.Id, WorkInstructionId = wiA.Id, OperatorId = "op1", CreatedBy = "op1",
            LogSteps =
            [
                new ProductionLogStep { WorkInstructionStepId = stepA1.Id, Attempts = [failed] },
                new ProductionLogStep { WorkInstructionStepId = stepA2.Id, Attempts = [success, notRecorded] }
            ]
        };

        var single = new ProductionLogStepAttempt { Success = false, Notes = "", SubmitTime = now };
        var logSingle = new ProductionLog
        {
            ProductId = productOne.Id, WorkInstructionId = wiA.Id, OperatorId = "ghost", CreatedBy = "op1",
            LogSteps = [new ProductionLogStep { WorkInstructionStepId = stepA1.Id, Attempts = [single] }]
        };

        context.ProductionLogs.AddRange(logMulti, logSingle);
        context.SaveChanges();

        return new SeedData(
            productOne.Id, productTwo.Id, wiA.Id, wiB.Id, stepA1.Id, stepA2.Id, stepB1.Id,
            nounOne.Id, nounTwo.Id, nounB.Id, adjOne.Id, adjTwo.Id,
            logMulti.Id, logSingle.Id, failed.Id, success.Id, single.Id, notRecorded.Id);
    }

    private async Task<ReworkUpdateRequest> RequestFor(int attemptId, Action<ReworkEditValues> change, bool confirm = false)
    {
        var edit = await CreateService().GetForEditAsync(attemptId);
        Assert.NotNull(edit);
        var updated = edit!.Values with { };
        change(updated);
        return new ReworkUpdateRequest
        {
            AttemptId = attemptId,
            Original = edit.Values,
            OriginalLogLastModifiedOn = edit.LogLastModifiedOn,
            Updated = updated,
            ConfirmSharedLogChange = confirm
        };
    }

    private static ReworkQuery Query(Action<ReworkQuery>? configure = null)
    {
        var query = new ReworkQuery { SortBy = "id", SortDir = "asc" };
        configure?.Invoke(query);
        return query;
    }

    // ── GetPageAsync ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPageAsync_FilterId_ReturnsFullyPopulatedRow()
    {
        var page = await CreateService().GetPageAsync(Query(q => q.FilterId = _seed.AttemptFailed));

        var row = Assert.Single(page.Data);
        Assert.Equal(_seed.AttemptFailed, row.Id);
        Assert.Equal(_seed.LogMulti, row.ProductionLogId);
        Assert.False(row.Success);
        Assert.Equal("Cracked housing", row.Notes);
        Assert.Equal("Housing", row.NounName);
        Assert.Equal("Cracked", row.AdjectiveName);
        Assert.Equal("Prod One", row.ProductName);
        Assert.Equal("WI-A (1.0)", row.InstructionName);
        Assert.Equal("Ann Op", row.OperatorName);
        Assert.Equal("Step 1", row.StepName);
    }

    [Fact]
    public async Task GetPageAsync_UnknownId_ReturnsEmptyPage()
    {
        var page = await CreateService().GetPageAsync(Query(q => q.FilterId = 999_999));

        Assert.Empty(page.Data);
        Assert.Equal(0, page.Total);
    }

    [Fact]
    public async Task GetPageAsync_TextFilters_AreCaseInsensitiveContains()
    {
        var service = CreateService();

        var byNotes = await service.GetPageAsync(Query(q => q.FilterNotes = "CRACKED"));
        var byStep = await service.GetPageAsync(Query(q => q.FilterStep = "step 2"));
        var byOperator = await service.GetPageAsync(Query(q => q.FilterOperator = "ann"));
        var byInstruction = await service.GetPageAsync(Query(q => q.FilterInstruction = "(1.0)"));

        Assert.Equal([_seed.AttemptFailed], byNotes.Data.Select(r => r.Id));
        Assert.Equal([_seed.AttemptSuccess, _seed.AttemptNotRecorded], byStep.Data.Select(r => r.Id));
        Assert.Equal(3, byOperator.Total);
        Assert.Equal(4, byInstruction.Total);
    }

    [Fact]
    public async Task GetPageAsync_SuccessFilter_DistinguishesTrueFalseAndNull()
    {
        var service = CreateService();

        var successes = await service.GetPageAsync(Query(q => q.FilterSuccess = true));
        var failures = await service.GetPageAsync(Query(q => q.FilterSuccess = false));
        var notRecorded = await service.GetPageAsync(Query(q => q.FilterSuccessNotRecorded = true));

        Assert.Equal([_seed.AttemptSuccess], successes.Data.Select(r => r.Id));
        Assert.Equal([_seed.AttemptFailed, _seed.AttemptSingle], failures.Data.Select(r => r.Id));
        Assert.Equal([_seed.AttemptNotRecorded], notRecorded.Data.Select(r => r.Id));
    }

    [Fact]
    public async Task GetPageAsync_SortsDescendingAndClampsPaging()
    {
        var page = await CreateService().GetPageAsync(new ReworkQuery { SortBy = "id", SortDir = "desc", Page = 99, PageSize = 7 });

        Assert.Equal(50, page.PageSize);
        Assert.Equal(1, page.Page);
        Assert.Equal(4, page.Total);
        Assert.Equal(page.Data.Select(r => r.Id).OrderByDescending(id => id), page.Data.Select(r => r.Id));
    }

    [Fact]
    public async Task GetPageAsync_MissingOperatorAndDefectCodes_RenderEmptyStrings()
    {
        var page = await CreateService().GetPageAsync(Query(q => q.FilterId = _seed.AttemptSingle));

        var row = Assert.Single(page.Data);
        Assert.Equal(string.Empty, row.OperatorName);
        Assert.Equal(string.Empty, row.NounName);
        Assert.Equal(string.Empty, row.AdjectiveName);
    }

    [Fact]
    public async Task GetForEditAsync_ReturnsSiblingCounts()
    {
        var edit = await CreateService().GetForEditAsync(_seed.AttemptFailed);

        Assert.NotNull(edit);
        Assert.Equal(2, edit!.OtherAttemptCount);
        Assert.Equal(1, edit.OtherStepCount);
        Assert.False(edit.HasPartTraceability);
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_NotesOnly_ChangesAttemptAndStampsLog()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.Notes = "Housing replaced");

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.True(result.Success, result.Message);
        await using var context = NewContext();
        var attempt = await context.ProductionLogStepAttempts.SingleAsync(a => a.Id == _seed.AttemptFailed, Ct);
        var log = await context.ProductionLogs.SingleAsync(l => l.Id == _seed.LogMulti, Ct);
        Assert.Equal("Housing replaced", attempt.Notes);
        Assert.False(attempt.Success);
        Assert.Equal(_seed.NounOne, attempt.FailureNounId);
        Assert.Equal(Technician, log.LastModifiedBy);
        Assert.Equal("op1", log.OperatorId);
    }

    [Fact]
    public async Task UpdateAsync_SuccessChange_IsRejected()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.Success = true);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.Validation, result.ErrorCode);
        await using var context = NewContext();
        Assert.False((await context.ProductionLogStepAttempts.SingleAsync(a => a.Id == _seed.AttemptFailed, Ct)).Success);
    }

    [Fact]
    public async Task UpdateAsync_NounChange_ClearsUnlinkedAdjective()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.FailureNounId = _seed.NounTwo);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Message);
        await using var context = NewContext();
        var attempt = await context.ProductionLogStepAttempts.SingleAsync(a => a.Id == _seed.AttemptFailed, Ct);
        Assert.Equal(_seed.NounTwo, attempt.FailureNounId);
        Assert.Null(attempt.FailureAdjectiveId);
    }

    [Fact]
    public async Task UpdateAsync_RejectsStepOutsideInstruction()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.WorkInstructionStepId = _seed.StepB1);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.Validation, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_RejectsNounNotLinkedToInstruction()
    {
        var request = await RequestFor(_seed.AttemptFailed, v =>
        {
            v.FailureNounId = _seed.NounB;
            v.FailureAdjectiveId = null;
        });

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.Validation, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_RejectsAdjectiveNotLinkedToNoun()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.FailureAdjectiveId = _seed.AdjTwo);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.Validation, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_RejectsUnknownOperator()
    {
        var request = await RequestFor(_seed.AttemptSingle, v => v.OperatorId = "nobody");

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.Validation, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_AcceptsOperatorWithIsActiveFalse()
    {
        var request = await RequestFor(_seed.AttemptSingle, v => v.OperatorId = "op-old");

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.True(result.Success, result.Message);
        await using var context = NewContext();
        Assert.Equal("op-old", (await context.ProductionLogs.SingleAsync(l => l.Id == _seed.LogSingle, Ct)).OperatorId);
    }

    [Fact]
    public async Task UpdateAsync_RejectsProductNotAssociatedWithInstruction()
    {
        var request = await RequestFor(_seed.AttemptSingle, v => v.ProductId = _seed.ProductTwo);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.Validation, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_StepChange_JoinsExistingStepAndRemovesEmptyOne()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.WorkInstructionStepId = _seed.StepA2);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.True(result.Success, result.Message);
        await using var context = NewContext();
        var steps = await context.ProductionLogSteps
            .Include(s => s.Attempts)
            .Where(s => s.ProductionLogId == _seed.LogMulti)
            .ToListAsync(Ct);
        var step = Assert.Single(steps);
        Assert.Equal(_seed.StepA2, step.WorkInstructionStepId);
        Assert.Equal(
            new[] { _seed.AttemptFailed, _seed.AttemptSuccess, _seed.AttemptNotRecorded }.Order(),
            step.Attempts.Select(a => a.Id).Order());
    }

    [Fact]
    public async Task UpdateAsync_StepChange_LeavesSiblingsOnOldStep()
    {
        var request = await RequestFor(_seed.AttemptSuccess, v => v.WorkInstructionStepId = _seed.StepA1);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.True(result.Success, result.Message);
        await using var context = NewContext();
        var steps = await context.ProductionLogSteps
            .Include(s => s.Attempts)
            .Where(s => s.ProductionLogId == _seed.LogMulti)
            .ToListAsync(Ct);
        Assert.Equal(2, steps.Count);
        var oldStep = steps.Single(s => s.WorkInstructionStepId == _seed.StepA2);
        Assert.Equal([_seed.AttemptNotRecorded], oldStep.Attempts.Select(a => a.Id));
    }

    [Fact]
    public async Task UpdateAsync_StepChange_CreatesMissingStep()
    {
        var request = await RequestFor(_seed.AttemptSingle, v => v.WorkInstructionStepId = _seed.StepA2);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.True(result.Success, result.Message);
        await using var context = NewContext();
        var step = await context.ProductionLogSteps
            .Include(s => s.Attempts)
            .SingleAsync(s => s.ProductionLogId == _seed.LogSingle, Ct);
        Assert.Equal(_seed.StepA2, step.WorkInstructionStepId);
        Assert.Equal([_seed.AttemptSingle], step.Attempts.Select(a => a.Id));
    }

    [Fact]
    public async Task UpdateAsync_OperatorChangeWithSiblings_RequiresConfirmation()
    {
        var unconfirmed = await RequestFor(_seed.AttemptFailed, v => v.OperatorId = "op2");
        var rejected = await CreateService().UpdateAsync(unconfirmed, Technician);
        Assert.Equal(ReworkUpdateErrorCode.Validation, rejected.ErrorCode);

        var confirmed = await RequestFor(_seed.AttemptFailed, v => v.OperatorId = "op2", confirm: true);
        var result = await CreateService().UpdateAsync(confirmed, Technician);

        Assert.True(result.Success, result.Message);
        var sibling = await CreateService().GetPageAsync(Query(q => q.FilterId = _seed.AttemptSuccess));
        Assert.Equal("Bob Op", Assert.Single(sibling.Data).OperatorName);
    }

    [Fact]
    public async Task UpdateAsync_InstructionChange_BlockedWhenLogHasOtherSteps()
    {
        var request = await RequestFor(_seed.AttemptFailed, v =>
        {
            v.WorkInstructionId = _seed.WiB;
            v.WorkInstructionStepId = _seed.StepB1;
            v.FailureNounId = null;
            v.FailureAdjectiveId = null;
        }, confirm: true);

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.InstructionChangeBlocked, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_InstructionChange_AllowedForOnlyAttemptInLog()
    {
        var request = await RequestFor(_seed.AttemptSingle, v =>
        {
            v.WorkInstructionId = _seed.WiB;
            v.WorkInstructionStepId = _seed.StepB1;
            v.ProductId = _seed.ProductTwo;
        });

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.True(result.Success, result.Message);
        await using var context = NewContext();
        var log = await context.ProductionLogs.Include(l => l.LogSteps).SingleAsync(l => l.Id == _seed.LogSingle, Ct);
        Assert.Equal(_seed.WiB, log.WorkInstructionId);
        Assert.Equal(_seed.ProductTwo, log.ProductId);
        Assert.Equal(_seed.StepB1, Assert.Single(log.LogSteps).WorkInstructionStepId);
    }

    [Fact]
    public async Task UpdateAsync_InstructionChange_BlockedByPartTraceability()
    {
        await using (var context = NewContext())
        {
            var part = new SerializablePart { PartDefinition = new PartDefinition { Name = "Sub", Number = "S-1" }, SerialNumber = "SN1" };
            context.SerializableParts.Add(part);
            await context.SaveChangesAsync(Ct);
            context.ProductionLogParts.Add(new ProductionLogPart
            {
                ProductionLogId = _seed.LogSingle, SerializablePartId = part.Id, OperationType = PartOperationType.Produced
            });
            await context.SaveChangesAsync(Ct);
        }

        var request = await RequestFor(_seed.AttemptSingle, v =>
        {
            v.WorkInstructionId = _seed.WiB;
            v.WorkInstructionStepId = _seed.StepB1;
        });

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.PartTraceabilityBlocked, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_StaleSnapshot_ReturnsConflictAndChangesNothing()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.Notes = "Housing replaced");

        await using (var context = NewContext())
        {
            var attempt = await context.ProductionLogStepAttempts.SingleAsync(a => a.Id == _seed.AttemptFailed, Ct);
            attempt.FailureAdjectiveId = null;
            await context.SaveChangesAsync(Ct);
        }

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.Conflict, result.ErrorCode);
        await using var verify = NewContext();
        Assert.Equal("Cracked housing", (await verify.ProductionLogStepAttempts.SingleAsync(a => a.Id == _seed.AttemptFailed, Ct)).Notes);
    }

    [Fact]
    public async Task UpdateAsync_RejectedEdit_LeavesNoPartialChange()
    {
        // The notes are valid but the step is not: nothing may be written.
        var request = await RequestFor(_seed.AttemptFailed, v =>
        {
            v.Notes = "Housing replaced";
            v.WorkInstructionStepId = _seed.StepB1;
        });

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.False(result.Success);
        await using var context = NewContext();
        Assert.Equal("Cracked housing", (await context.ProductionLogStepAttempts.SingleAsync(a => a.Id == _seed.AttemptFailed, Ct)).Notes);
    }

    [Fact]
    public async Task UpdateAsync_OperatorCaller_IsForbidden()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.Notes = "Housing replaced");

        var result = await CreateService().UpdateAsync(request, "op1");

        Assert.Equal(ReworkUpdateErrorCode.Forbidden, result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_UnknownAttempt_ReturnsNotFound()
    {
        var request = await RequestFor(_seed.AttemptFailed, v => v.Notes = "Housing replaced");
        request.AttemptId = 999_999;

        var result = await CreateService().UpdateAsync(request, Technician);

        Assert.Equal(ReworkUpdateErrorCode.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task GetStepsForInstructionAsync_ReturnsStepsInPositionOrder()
    {
        var steps = await CreateService().GetStepsForInstructionAsync(_seed.WiA);

        Assert.Equal(["Step 1", "Step 2"], steps.Select(s => s.Name));
    }
}
