using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.DTOs.WorkInstructions.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.PartNodes.Form;
using MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.Form;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.CRUD.WorkInstructions;

/// <summary>
/// Provides logic for applying changes from a <see cref="WorkInstructionFormDTO"/>
/// to an existing <see cref="WorkInstruction"/> aggregate.
/// </summary>
/// <remarks>
/// This class is responsible for synchronizing scalar properties and
/// related collections such as nodes and products while respecting
/// Entity Framework Core tracking rules.  
/// 
/// It mutates tracked entities in place and does not persist changes;
/// callers are responsible for invoking <c>SaveChanges</c>.
/// </remarks>
public class WorkInstructionUpdater : IWorkInstructionUpdater
{
    /// <summary>
    /// Applies the values from a <see cref="WorkInstructionFormDTO"/> to an existing
    /// tracked <see cref="WorkInstruction"/> entity.
    /// </summary>
    /// <remarks>
    /// This method mutates the provided <paramref name="entity"/> in place.
    /// The entity must already be loaded from the database with its related
    /// <see cref="WorkInstruction.Nodes"/> and <see cref="WorkInstruction.Products"/>
    /// collections included and tracked by the provided <paramref name="context"/>.
    /// <para>
    /// The method performs:
    /// <list type="bullet">
    /// <item>
    /// Updates of scalar properties (e.g., title, version, flags).
    /// </item>
    /// <item>
    /// Synchronization of the many-to-many <see cref="WorkInstruction.Products"/> collection.
    /// </item>
    /// <item>
    /// Synchronization of the <see cref="WorkInstruction.Nodes"/> collection,
    /// including creation, update, and removal of nodes.
    /// </item>
    /// </list>
    /// </para>
    /// This method does not call <c>SaveChanges</c>; the caller is responsible
    /// for persisting changes.
    /// </remarks>
    /// <param name="dto">
    /// The form DTO containing the desired state of the work instruction.
    /// </param>
    /// <param name="entity">
    /// The existing tracked work instruction entity to mutate.
    /// </param>
    /// <param name="context">
    /// The active <see cref="ApplicationContext"/> used for resolving
    /// related entities and tracking changes.
    /// </param>
    /// <returns>
    /// A task that completes when the mutation process has finished.
    /// </returns>
    public async Task ApplyAsync(
        WorkInstructionFormDTO dto,
        WorkInstruction entity,
        ApplicationContext context)
    {
        ApplyScalars(dto, entity);

        await SyncPartProducedAsync(dto, entity, context);
        await SyncProductsAsync(dto, entity, context);

        SyncNodes(dto.Nodes, entity, context);
    }
    
    private void ApplyScalars(WorkInstructionFormDTO dto, WorkInstruction entity)
    {
        entity.Title = dto.Title;
        entity.Version = dto.Version;
        entity.IsActive = dto.IsActive;
        entity.ShouldGenerateQrCode = dto.ShouldGenerateQrCode;
        entity.PartProducedIsSerialized = dto.PartProducedIsSerialized;
        entity.PartProducedId = dto.PartProducedId;
    }

    private async Task SyncProductsAsync(
        WorkInstructionFormDTO dto,
        WorkInstruction entity,
        ApplicationContext context)
    {
        var existingIds = entity.Products.Select(p => p.Id).ToHashSet();
        var incomingIds = dto.ProductIds.ToHashSet();

        // Remove
        entity.Products.RemoveAll(p => !incomingIds.Contains(p.Id));

        // Add
        var toAdd = incomingIds.Except(existingIds).ToList();

        if (toAdd.Count > 0)
        {
            var products = await context.Products
                .Where(p => toAdd.Contains(p.Id))
                .ToListAsync();

            entity.Products.AddRange(products);
        }
    }
    
    private void SyncNodes(
        List<WorkInstructionNodeFormDTO> dtos,
        WorkInstruction entity,
        ApplicationContext context)
    {
        var existingById = entity.Nodes.ToDictionary(n => n.Id);

        var incomingIds = dtos
            .Where(d => d.Id != 0)   // 0 means “new node”
            .Select(d => d.Id)
            .ToHashSet();

        // ---- Remove deleted ----
        var toRemove = entity.Nodes
            .Where(n => !incomingIds.Contains(n.Id))
            .ToList();

        foreach (var node in toRemove)
            context.Remove(node);

        // ---- Add / Update ----
        foreach (var dto in dtos)
        {
            if (dto.Id != 0 && existingById.TryGetValue(dto.Id, out var existing))
            {
                ApplyToExisting(dto, existing);
            }
            else
            {
                var newNode = CreateNew(dto);
                entity.Nodes.Add(newNode);
            }
        }
    }
    
    private async Task SyncPartProducedAsync(
        WorkInstructionFormDTO dto,
        WorkInstruction entity,
        ApplicationContext context)
    {
        if (dto.PartProducedId.HasValue)
        {
            entity.PartProduced = await context.PartDefinitions
                .FirstOrDefaultAsync(p => p.Id == dto.PartProducedId.Value);
        }
        else
        {
            entity.PartProduced = null;
        }
    }
    
    private void ApplyToExisting(
        WorkInstructionNodeFormDTO dto,
        WorkInstructionNode entity)
    {
        entity.Position = dto.Position;

        switch (dto)
        {
            case StepNodeFormDTO stepDto when entity is Step step:
                ApplyStep(stepDto, step);
                break;

            case PartNodeFormDTO partDto when entity is PartNode part:
                ApplyPartNode(partDto, part);
                break;

            default:
                throw new InvalidOperationException("Node type mismatch.");
        }
    }
    
    private WorkInstructionNode CreateNew(WorkInstructionNodeFormDTO dto)
    {
        return dto switch
        {
            StepNodeFormDTO stepDto => (WorkInstructionNode)new Step
            {
                NodeType = WorkInstructionNodeType.Step,
                Position = stepDto.Position,
                Name = stepDto.Name,
                Body = stepDto.Body,
                DetailedBody = stepDto.DetailedBody,
                PrimaryMedia = stepDto.PrimaryMedia.ToList(),
                SecondaryMedia = stepDto.SecondaryMedia.ToList()
            },

            PartNodeFormDTO partDto => CreatePartNode(partDto),

            _ => throw new NotSupportedException()
        };
    }
    
    private void ApplyStep(StepNodeFormDTO dto, Step entity)
    {
        entity.Name = dto.Name;
        entity.Body = dto.Body;
        entity.DetailedBody = dto.DetailedBody;

        entity.PrimaryMedia = dto.PrimaryMedia.ToList();
        entity.SecondaryMedia = dto.SecondaryMedia.ToList();
    }
    
    private void ApplyPartNode(PartNodeFormDTO dto, PartNode entity)
    {
        entity.PartDefinitionId = dto.PartDefinitionId;
        entity.InputType = dto.InputType;
    }
    
    private PartNode CreatePartNode(PartNodeFormDTO partDto)
    {
        var node = new PartNode
        {
            NodeType = WorkInstructionNodeType.Part,
            Position = partDto.Position,
            InputType = partDto.InputType
        };

        if (partDto.PartDefinitionId > 0)
        {
            node.PartDefinitionId = partDto.PartDefinitionId;
        }
        else if (partDto.PartDefinition != null)
        {
            node.PartDefinition = new PartDefinition
            {
                Name = partDto.PartDefinition.Name,
                Number = partDto.PartDefinition.Number
            };
        }
        else
        {
            throw new InvalidOperationException(
                "PartNode must have either PartDefinitionId or PartDefinition.");
        }

        return node;
    }
}