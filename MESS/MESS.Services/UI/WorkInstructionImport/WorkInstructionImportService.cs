using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.File;
using MESS.Services.DTOs.WorkInstructions.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.File;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.File;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.UI.WorkInstructionImport;

/// <inheritdoc/>
public class WorkInstructionImportService : IWorkInstructionImportService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    /// <summary>
    /// Constructs the WorkInstructionImportService with the necessary dependencies.
    /// </summary>
    /// <param name="contextFactory"></param>
    public WorkInstructionImportService(
        IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    /// <inheritdoc/>
    public async Task<WorkInstructionImportApplicationResult> ImportAsync(
    WorkInstructionFileDTO fileDto)
    {
        var result = new WorkInstructionImportApplicationResult();

        await using var context = await _contextFactory.CreateDbContextAsync();

        var parts = await context.PartDefinitions
            .AsNoTracking()
            .ToListAsync();

        var products = await context.Products
            .Include(p => p.PartDefinition)
            .AsNoTracking()
            .ToListAsync();

        var errors = new List<string>();

        // -----------------------------
        // Resolve Produced Part
        // -----------------------------

        int? producedPartId = null;

        if (!string.IsNullOrWhiteSpace(fileDto.ProducedPartName))
        {
            var part = parts
                .FirstOrDefault(p => p.Name == fileDto.ProducedPartName);

            if (part == null)
            {
                errors.Add(
                    $"Produced part '{fileDto.ProducedPartName}' was not found.");
            }
            else
            {
                producedPartId = part.Id;
            }
        }

        // -----------------------------
        // Resolve Products
        // -----------------------------

        var resolvedProductIds = new List<int>();

        foreach (var productName in fileDto.AssociatedProductNames)
        {
            var product = products
                .FirstOrDefault(p => p.PartDefinition.Name == productName);

            if (product == null)
            {
                errors.Add($"Product '{productName}' was not found.");
                continue;
            }

            resolvedProductIds.Add(product.Id);
        }

        // -----------------------------
        // Resolve Nodes
        // -----------------------------

        var resolvedNodes = new List<WorkInstructionNodeFormDTO>();

        foreach (var node in fileDto.Nodes.OrderBy(n => n.Position))
        {
            var mappedNode = MapNode(node, parts, errors);

            if (mappedNode != null)
            {
                resolvedNodes.Add(mappedNode);
            }
        }

        if (errors.Any())
        {
            result.Errors = errors;
            result.Success = false;
            return result;
        }

        var formDto = new WorkInstructionFormDTO
        {
            Id = null,
            OriginalId = null,
            IsLatest = true,

            Title = fileDto.Title,
            Version = fileDto.Version,
            IsActive = fileDto.IsActive,
            ShouldGenerateQrCode = fileDto.ShouldGenerateQrCode,
            PartProducedIsSerialized = fileDto.PartProducedIsSerialized,

            ProducedPartName = fileDto.ProducedPartName,
            PartProducedId = producedPartId,

            ProductIds = resolvedProductIds,
            Nodes = resolvedNodes
        };

        result.Success = true;
        result.WorkInstruction = formDto;

        return result;
    }
    
    private WorkInstructionNodeFormDTO? MapNode(
        WorkInstructionNodeFileDTO nodeDto,
        List<PartDefinition> allParts,
        List<string> errors)
    {
        return nodeDto switch
        {
            PartNodeFileDTO partDto => allParts
                .FirstOrDefault(p => p.Name == partDto.PartName) is { } part
                ? partDto.ToFormDTO(part)
                : null,
            StepNodeFileDTO stepDto => stepDto.ToFormDTO(),
            _ => null
        };
    }
}