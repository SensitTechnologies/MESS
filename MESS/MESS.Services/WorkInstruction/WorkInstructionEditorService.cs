namespace MESS.Services.WorkInstruction;
using MESS.Data.Models;

/// <inheritdoc />
public class WorkInstructionEditorService : IWorkInstructionEditorService
{
    private readonly IWorkInstructionService _workInstructionService;
    /// <inheritdoc />
    public WorkInstruction? Current { get; private set; }
    /// <inheritdoc />
    public bool IsDirty { get; private set; }
    /// <inheritdoc />
    public EditorMode Mode { get; private set; } = EditorMode.None;

    /// <inheritdoc />
    public event Action? OnChanged;

    /// <inheritdoc />
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
            CollectsProductSerialNumber = false,
            Products = products ?? new List<Product>(),
            Nodes = new List<WorkInstructionNode>()
        };

        Mode = EditorMode.CreateNew;
        IsDirty = true;
        NotifyChanged();
    }
    
    /// <inheritdoc />
    public void StartNewFromCurrent(string? title = null, List<Product>? products = null)
    {
        if (Current == null)
            throw new InvalidOperationException("Cannot start a new work instruction from current because it is null.");

        var newInstruction = new WorkInstruction
        {
            Title = title ?? Current.Title,
            Version = "1.0",
            OriginalId = Current.OriginalId ?? Current.Id,
            IsActive = false,
            IsLatest = true,
            ShouldGenerateQrCode = Current.ShouldGenerateQrCode,
            CollectsProductSerialNumber = Current.CollectsProductSerialNumber,
            Products = products ?? Current.Products?.ToList() ?? new List<Product>(),
            Nodes = Current.Nodes.Select(CloneNode).ToList()
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

        Current = CloneForNewVersion(template);
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
        var newVersion = CloneForNewVersion(oldVersion);
        
        newVersion.Version = oldVersion.Version;

        // Assign OriginalId from the old version
        newVersion.OriginalId = oldVersion.OriginalId;

        // Set mode and mark as dirty
        Current = newVersion;
        Mode = EditorMode.CreateNewVersion;
        IsDirty = true;

        NotifyChanged();
    }
    
    private WorkInstruction CloneForNewVersion(WorkInstruction template)
    {
        return new WorkInstruction
        {
            Title = template.Title,
            Version = template.Version,
            OriginalId = template.OriginalId ?? template.Id,
            IsActive = false,
            IsLatest = true,
            ShouldGenerateQrCode = template.ShouldGenerateQrCode,
            CollectsProductSerialNumber = template.CollectsProductSerialNumber,
            Products = template.Products?.ToList() ?? new List<Product>(),
            Nodes = template.Nodes
                .Select(CloneNode)
                .ToList()
        };
    }

    private WorkInstructionNode CloneNode(WorkInstructionNode node)
    {
        if (node is PartNode partNode)
        {
            return new PartNode
            {
                Parts = partNode.Parts
                    .Select(p => new Part
                    {
                        PartName = p.PartName,
                        PartNumber = p.PartNumber
                    })
                    .ToList()
            };
        }
        else if (node is Step stepNode)
        {
            return new Step
            {
                Name = stepNode.Name,
                Body = stepNode.Body,
                DetailedBody = stepNode.DetailedBody,
                PrimaryMedia = stepNode.PrimaryMedia?.ToList() ?? new List<string>(),
                SecondaryMedia = stepNode.SecondaryMedia?.ToList() ?? new List<string>()
            };
        }

        throw new NotSupportedException("Unknown WorkInstructionNode type");
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
                break;

            case EditorMode.EditExisting:
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
        if (success && Current.IsActive)
        {
            await _workInstructionService.MarkOtherVersionsInactiveAsync(Current.Id);
        }
        
        if (success)
        {
            IsDirty = false;
            NotifyChanged();
        }

        return success;
    }

    /// <inheritdoc />
    public void RevertChanges()
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
}
