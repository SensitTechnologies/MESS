using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.Form;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.CRUD.WorkInstructions;

/// <inheritdoc/>
public class PartNodeResolver : IPartNodeResolver
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartNodeResolver"/> class.
    /// </summary>
    /// <param name="contextFactory">
    /// An <see cref="IDbContextFactory{ApplicationContext}"/> used to create
    /// <see cref="ApplicationContext"/> instances for database access.
    /// </param>
    /// <remarks>
    /// This resolver uses the context to look up existing <see cref="PartDefinition"/>s
    /// and create new ones as needed when resolving <see cref="PartNode"/>s
    /// prior to saving a work instruction.
    /// </remarks>
    public PartNodeResolver(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc/>
    public async Task ResolvePendingNodesAsync(IEnumerable<WorkInstructionNode> nodes)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var partNodes = nodes
            .OfType<PartNode>()
            .Where(n => n.PartDefinitionId == 0 || n.PartDefinition != null)
            .ToList();

        if (!partNodes.Any())
            return;

        var existingParts = await context.PartDefinitions
            .AsNoTracking()
            .ToListAsync();

        foreach (var node in partNodes)
        {
            if (node.PartDefinition is null)
                throw new InvalidOperationException("PartNode has no PartName/PartNumber.");

            var partName = node.PartDefinition.Name.Trim();
            var partNumber = node.PartDefinition.Number?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(partName))
                throw new InvalidOperationException("PartNode has no PartName specified.");

            var existing = existingParts.FirstOrDefault(p =>
                p.Name.Trim().Equals(partName, StringComparison.OrdinalIgnoreCase) &&
                (p.Number?.Trim() ?? string.Empty).Equals(partNumber, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                node.PartDefinitionId = existing.Id;
                node.PartDefinition = null; // EF will resolve via FK
            }
            else
            {
                var newPart = new PartDefinition
                {
                    Name = partName,
                    Number = partNumber
                };

                context.PartDefinitions.Add(newPart);
                node.PartDefinition = newPart; // EF will track the new part
            }
        }
    }
}