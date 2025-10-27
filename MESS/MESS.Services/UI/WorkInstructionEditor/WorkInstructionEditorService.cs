using MESS.Services.CRUD.WorkInstructions;
using Serilog;

namespace MESS.Services.UI.WorkInstructionEditor;
using Data.Models;
using System.Threading.Tasks;

/// <inheritdoc />
public class WorkInstructionEditorService : IWorkInstructionEditorService
{
    private readonly IWorkInstructionService _workInstructionService;
    /// <inheritdoc />
    public WorkInstruction? Current { get; private set; }
    
    private readonly List<WorkInstructionNode> _nodesQueuedForDeletion = [];
    
    /// <summary>
    /// Gets a read-only list of <see cref="WorkInstructionNode"/> instances
    /// that have been queued for deletion but not yet removed from the database.
    /// </summary>
    public IReadOnlyList<WorkInstructionNode> NodesQueuedForDeletion => _nodesQueuedForDeletion.AsReadOnly();

    /// <inheritdoc />
    public bool CurrentHasParts()
    {
        if (Current == null) return false;

        if (Current.Nodes.Count == 0) return false;
        
        foreach (var node in Current.Nodes)
        {
            if (node is PartNode)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <inheritdoc />
    public bool CurrentHasSteps()
    {
        if (Current == null) return false;

        if (Current.Nodes.Count == 0) return false;
        
        foreach (var node in Current.Nodes)
        {
            if (node is Step)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <inheritdoc />
    public bool IsDirty { get; private set; }
    /// <inheritdoc />
    public EditorMode Mode { get; private set; } = EditorMode.None;

    /// <inheritdoc />
    public event Action? OnChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkInstructionEditorService"/> class.
    /// </summary>
    /// <param name="workInstructionService">
    /// The service used to retrieve, create, update, and clone <see cref="WorkInstruction"/> 
    /// entities during the editing process.
    /// </param>
    public WorkInstructionEditorService(IWorkInstructionService workInstructionService)
    {
        _workInstructionService = workInstructionService;
    }

    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }

    /// <inheritdoc />
    public void MarkDirty()
    {
        IsDirty = true;
        NotifyChanged();
    }

    /// <inheritdoc />
    public void StartNew(string? title = null, List<Product>? products = null)
    {
        Current = new WorkInstruction
        {
            Title = title ?? "",
            Version = "1.0",
            IsActive = false,
            IsLatest = true,
            ShouldGenerateQrCode = false,
            PartProducedIsSerialized = false,
            Products = products ?? new List<Product>(),
            Nodes = new List<WorkInstructionNode>()
        };

        Mode = EditorMode.CreateNew;
        IsDirty = true;
        NotifyChanged();
    }
    
    /// <inheritdoc />
    public async Task StartNewFromCurrent(string? title = null, List<Product>? products = null)
    {
        if (Current == null)
            throw new InvalidOperationException("Cannot start a new work instruction from current because it is null.");

        var newInstruction = new WorkInstruction
        {
            Title = title ?? Current.Title,
            Version = "1.0",
            IsActive = false,
            IsLatest = true,
            ShouldGenerateQrCode = Current.ShouldGenerateQrCode,
            PartProducedIsSerialized = Current.PartProducedIsSerialized,
            Products = products ?? Current.Products?.ToList() ?? new List<Product>(),
            Nodes = await CloneNodesAsync(Current.Nodes)
        };

        Current = newInstruction;
        Mode = EditorMode.CreateNew;
        IsDirty = true;
        NotifyChanged();
    }
    
    /// <inheritdoc />
    public async Task LoadForEditAsync(int id)
    {
        var wi = await _workInstructionService.GetByIdAsync(id);
        if (wi == null)
            throw new Exception($"WorkInstruction with ID {id} not found.");

        Current = wi;
        Mode = EditorMode.EditExisting;
        IsDirty = false;
        NotifyChanged();
    }
    
    /// <inheritdoc />
    public async Task LoadForNewVersionFromCurrentAsync()
    {
        if (Current != null)
        {
          Current = await CloneForNewVersion(Current);
          Mode = EditorMode.CreateNewVersion;
          IsDirty = true;
          NotifyChanged();          
        }
    }

    
    /// <inheritdoc />
    public async Task LoadForNewVersionAsync(int originalId)
    {
        var latest = await _workInstructionService.GetAllAsync();
        
        // Finds the latest version in the same version chain as the given originalId.
        // Matches any work instruction that is marked as IsLatest and belongs to the chain
        // identified by originalId. We allow matching either OriginalId or Id because
        // the very first version in a chain has OriginalId == Id.
        var template = latest
            .FirstOrDefault(w => w.IsLatest && (w.OriginalId == originalId || w.Id == originalId));

        if (template == null)
            throw new Exception($"No latest version found for OriginalId {originalId}.");

        Current = await CloneForNewVersion(template);
        Mode = EditorMode.CreateNewVersion;
        IsDirty = true;
        NotifyChanged();
    }
    
    /// <inheritdoc />
    public async Task LoadForNewVersionFromVersionAsync(int versionId)
    {
        // Load the version to restore by ID
        var oldVersion = await _workInstructionService.GetByIdAsync(versionId);
        if (oldVersion == null)
            throw new Exception($"Version with ID {versionId} not found.");

        // Clone it
        var newVersion = await CloneForNewVersion(oldVersion);
        
        newVersion.Version = oldVersion.Version;

        // Assign OriginalId from the old version
        newVersion.OriginalId = oldVersion.OriginalId;

        // Set mode and mark as dirty
        Current = newVersion;
        Mode = EditorMode.CreateNewVersion;
        IsDirty = true;

        NotifyChanged();
    }
    
    private async Task<WorkInstruction> CloneForNewVersion(WorkInstruction template)
    {
        return new WorkInstruction
        {
            Title = template.Title,
            Version = template.Version,
            OriginalId = template.OriginalId ?? template.Id,
            IsActive = false,
            IsLatest = true,
            ShouldGenerateQrCode = template.ShouldGenerateQrCode,
            PartProducedIsSerialized = template.PartProducedIsSerialized,
            Products = template.Products?.ToList() ?? new List<Product>(),
            Nodes = await CloneNodesAsync(template.Nodes)
        };
    }

    private async Task<List<WorkInstructionNode>> CloneNodesAsync(List<WorkInstructionNode> nodes)
    {
        if (nodes.Count == 0)
        {
            return nodes;
        }

        var clone = new List<WorkInstructionNode>();
        foreach (var node in nodes)
        {
            clone.Add(await CloneNodeAsync(node));
        }

        return clone?.ToList() ?? new List<WorkInstructionNode>();
    }

    private async Task<WorkInstructionNode> CloneNodeAsync(WorkInstructionNode node)
    {
        if (node is PartNode partNode)
        {
            return new PartNode
            {
                NodeType = WorkInstructionNodeType.Part,
                Parts = partNode.Parts
                    .Select(p => new PartDefinition
                    {
                        PartName = p.Name,
                        PartNumber = p.Number
                    })
                    .ToList()
            };
        }
        else if (node is Step stepNode)
        {
            return new Step
            {
                NodeType = WorkInstructionNodeType.Step,
                Name = stepNode.Name,
                Body = stepNode.Body,
                Position = stepNode.Position,
                DetailedBody = stepNode.DetailedBody,
                PrimaryMedia = (await CloneImages(stepNode.PrimaryMedia))?.ToList() ?? new List<string>(),
                SecondaryMedia = (await CloneImages(stepNode.SecondaryMedia))?.ToList() ?? new List<string>()
            };
        }

        throw new NotSupportedException("Unknown WorkInstructionNode type");
    }

    private async Task<List<string>> CloneImages(List<string> Images)
    {

        if (Images.Count == 0)
        {
            return Images;
        }

        var clone = new List<string>();
        foreach (var image in Images) 
        {
            clone.Add( await _workInstructionService.SaveImageFileAsync(image));
        }


        return clone?.ToList() ?? new List<string>();
    }

    private string IncrementVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return "1.0";

        var parts = version.Split('.');
        if (parts.Length == 2 && int.TryParse(parts[1], out int minor))
        {
            return $"{parts[0]}.{minor + 1}";
        }

        return version + ".1";
    }

    /// <inheritdoc />
    public async Task<bool> SaveAsync()
    {
        if (Current == null)
            return false;

        bool success = false;

        switch (Mode)
        {
            case EditorMode.CreateNew:
                Current.OriginalId = null;
                Current.IsLatest = true;
                success = await _workInstructionService.Create(Current);
                Mode = EditorMode.EditExisting;
                break;

            case EditorMode.EditExisting:
                Current.IsLatest = true;
                success = await _workInstructionService.UpdateWorkInstructionAsync(Current);
                break;

            case EditorMode.CreateNewVersion:
                if (Current.OriginalId == null)
                    throw new InvalidOperationException("OriginalId is required for versioning.");

                await _workInstructionService.MarkAllVersionsNotLatestAsync(Current.OriginalId.Value);
                Current.IsLatest = true;
                success = await _workInstructionService.Create(Current);
                break;
        }
        if (Current != null)
        {
            if (success && Current.IsActive)
            {
                await _workInstructionService.MarkOtherVersionsInactiveAsync(Current.Id);
            }
        }
        else
        {
            Log.Warning("Current Work Instruction in editor is null.");
        }
        if (success)
        {
            IsDirty = false;
            NotifyChanged();
            
            // after saving Current, delete nodes to prevent orphaning
            if (_nodesQueuedForDeletion.Any())
            {
                var deleted = await _workInstructionService.DeleteNodesAsync(_nodesQueuedForDeletion);
                if (deleted)
                {
                    _nodesQueuedForDeletion.Clear();
                }
                else
                {
                    // handle failure to delete nodes, maybe log warning or throw
                }
            }
        }

        return success;
    }

    /// <inheritdoc />
    public void Reset()
    {
        Current = null;
        IsDirty = false;
        Mode = EditorMode.None;
        NotifyChanged();
    }
    
    /// <inheritdoc />
    public void ToggleActive()
    {
        if (Current == null) return;
        Current.IsActive = !Current.IsActive;
        MarkDirty();
    }
    
    /// <inheritdoc />
    public void QueueNodeForDeletion(WorkInstructionNode node)
    {
        if (node == null) return;
        if (!_nodesQueuedForDeletion.Contains(node))
        {
            _nodesQueuedForDeletion.Add(node);
        }
    }
}
