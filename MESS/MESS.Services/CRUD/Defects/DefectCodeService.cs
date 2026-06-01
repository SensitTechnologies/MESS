using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.DTOs.Defects;
using MESS.Services.DTOs.FailureAdjectives;
using MESS.Services.DTOs.FailureNouns;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.CRUD.Defects;

/// <inheritdoc />
public class DefectCodeService(IDbContextFactory<ApplicationContext> contextFactory) : IDefectCodeService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<DefectCodeOptionDto>> GetNounsForWorkInstructionAsync(int workInstructionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FailureNouns
            .AsNoTracking()
            .Where(n => n.WorkInstructions.Any(w => w.Id == workInstructionId))
            .OrderBy(n => n.Name)
            .Select(n => new DefectCodeOptionDto { Id = n.Id, Name = n.Name })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DefectCodeOptionDto>> GetAdjectivesForNounAsync(int nounId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FailureNouns
            .AsNoTracking()
            .Where(n => n.Id == nounId)
            .SelectMany(n => n.Adjectives)
            .OrderBy(a => a.Name)
            .Select(a => new DefectCodeOptionDto { Id = a.Id, Name = a.Name })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FailureNounAdminDto>> GetAllNounsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FailureNouns
            .AsNoTracking()
            .Include(n => n.Adjectives)
            .OrderBy(n => n.Name)
            .Select(n => new FailureNounAdminDto
            {
                Id = n.Id,
                Name = n.Name,
                AdjectiveIds = n.Adjectives.Select(a => a.Id).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FailureAdjectiveAdminDto>> GetAllAdjectivesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.FailureAdjectives
            .AsNoTracking()
            .Include(a => a.Nouns)
            .OrderBy(a => a.Name)
            .Select(a => new FailureAdjectiveAdminDto
            {
                Id = a.Id,
                Name = a.Name,
                NounIds = a.Nouns.Select(n => n.Id).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CreateNounAsync(FailureNounCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var noun = new FailureNoun { Name = request.Name.Trim() };
        if (request.AdjectiveIds?.Count > 0)
        {
            var adj = await context.FailureAdjectives
                .Where(a => request.AdjectiveIds.Contains(a.Id))
                .ToListAsync(cancellationToken);
            foreach (var a in adj)
                noun.Adjectives.Add(a);
        }

        context.FailureNouns.Add(noun);
        await context.SaveChangesAsync(cancellationToken);
        return noun.Id;
    }

    /// <inheritdoc />
    public async Task UpdateNounAsync(FailureNounUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var noun = await context.FailureNouns
            .Include(n => n.Adjectives)
            .Include(n => n.WorkInstructions)
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);
        if (noun is null)
            return;

        noun.Name = request.Name.Trim();
        noun.Adjectives.Clear();
        if (request.AdjectiveIds?.Count > 0)
        {
            var adj = await context.FailureAdjectives
                .Where(a => request.AdjectiveIds.Contains(a.Id))
                .ToListAsync(cancellationToken);
            foreach (var a in adj)
                noun.Adjectives.Add(a);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteNounAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var noun = await context.FailureNouns
            .Include(n => n.Adjectives)
            .Include(n => n.WorkInstructions)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (noun is null)
            return;

        noun.Adjectives.Clear();
        noun.WorkInstructions.Clear();
        context.FailureNouns.Remove(noun);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CreateAdjectiveAsync(FailureAdjectiveCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var adj = new FailureAdjective { Name = request.Name.Trim() };
        if (request.NounIds?.Count > 0)
        {
            var nouns = await context.FailureNouns
                .Where(n => request.NounIds.Contains(n.Id))
                .ToListAsync(cancellationToken);
            foreach (var n in nouns)
                adj.Nouns.Add(n);
        }

        context.FailureAdjectives.Add(adj);
        await context.SaveChangesAsync(cancellationToken);
        return adj.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAdjectiveAsync(FailureAdjectiveUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var adj = await context.FailureAdjectives
            .Include(a => a.Nouns)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (adj is null)
            return;

        adj.Name = request.Name.Trim();
        adj.Nouns.Clear();
        if (request.NounIds?.Count > 0)
        {
            var nouns = await context.FailureNouns
                .Where(n => request.NounIds.Contains(n.Id))
                .ToListAsync(cancellationToken);
            foreach (var n in nouns)
                adj.Nouns.Add(n);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAdjectiveAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var adj = await context.FailureAdjectives
            .Include(a => a.Nouns)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (adj is null)
            return;

        adj.Nouns.Clear();
        context.FailureAdjectives.Remove(adj);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> GetNounIdsForWorkInstructionAsync(int workInstructionId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.WorkInstructions
            .AsNoTracking()
            .Where(w => w.Id == workInstructionId)
            .SelectMany(w => w.FailureNouns)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetNounsForWorkInstructionAsync(int workInstructionId, IReadOnlyList<int> nounIds, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var wi = await context.WorkInstructions
            .Include(w => w.FailureNouns)
            .FirstOrDefaultAsync(w => w.Id == workInstructionId, cancellationToken);
        if (wi is null)
            return;

        wi.FailureNouns.Clear();
        if (nounIds.Count > 0)
        {
            var nouns = await context.FailureNouns
                .Where(n => nounIds.Contains(n.Id))
                .ToListAsync(cancellationToken);
            foreach (var n in nouns)
                wi.FailureNouns.Add(n);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
