using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QRCoder;
using Serilog;
namespace MESS.Blazor.Components.Pages.ProductionLog;

using Data.Models;

internal enum Status
{
    NotStarted,
    InProgress,
    Completed
}
/// <summary>
/// Represents the Create component for managing production logs.
/// This component handles the creation, modification, and submission of production logs.
/// </summary>
public partial class Create : ComponentBase, IAsyncDisposable
{
    private const string Title = "Production Log";
    private bool IsLoading { get; set; } = true;
    private ConfirmationModal? popupRef;
    private bool IsWorkflowActive { get; set; }
    private Status WorkInstructionStatus { get; set; } = Status.NotStarted;
    private bool IsSaved { get; set; }
    
    private Product? ActiveProduct { get; set; }
    private WorkInstruction? ActiveWorkInstruction { get; set; }

    /// <summary>
    /// Represents the current production logs being created or modified.
    /// The object in this collection hold all the details of the production log.
    /// </summary>
    protected ProductionLogBatch ProductionLogBatch = new();
    
    private List<Product>? Products { get; set; }
    private List<WorkInstruction>? WorkInstructions { get; set; }
    private List<WorkInstruction>? ActiveProductWorkInstructionList { get; set; }
    
    private string? ActiveLineOperator { get; set; }
    private string? ProductSerialNumber { get; set; }
    private string? QRCodeDataUrl;
    private IJSObjectReference? module;
    private List<ProductionLogPart> ProductionLogParts { get; set; } = [];
    
    private Func<List<ProductionLog>, Task>? _autoSaveHandler;
    
    private int BatchSize { get; set; } = 1;
    
    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        ProductionLogEventService.DisableAutoSave();
        await LoadProducts();
        await GetInProgressAsync();

         // Load cached forms as a list of ProductionLog
        ProductionLogBatch.Logs = await LoadCachedFormsAsync();

        bool cachedFormsLoaded = ProductionLogBatch.Logs != null && ProductionLogBatch.Logs.Count > 0;

        if (cachedFormsLoaded)
        {
            ProductionLogEventService.EnableAutoSave();
            await InvokeAsync(StateHasChanged);
        }

        if (ProductionLogBatch.Logs != null)
        {
            await ProductionLogEventService.SetCurrentProductionLogs(ProductionLogBatch.Logs);
        }
        
        // AutoSave Trigger
        _autoSaveHandler = async logs =>
        {
            if (logs == null || logs.Count == 0)
            {
                Log.Warning("Attempted to autosave an empty or null production log list.");
                return;
            }

            await LocalCacheManager.SetProductionLogBatchAsync(logs);

            await InvokeAsync(() =>
            {
                IsSaved = true;
                StateHasChanged();
            });
        };
        
        var result = await AuthProvider.GetAuthenticationStateAsync();
        ActiveLineOperator = result.User.Identity?.Name;
        // This must come before the LoadCachedForm method since if it finds a cached form, it will set the status to InProgress
        WorkInstructionStatus = Status.NotStarted;
        
        ProductionLogEventService.AutoSaveTriggered += _autoSaveHandler;
        ProductSerialNumber = SerializationService.CurrentProductNumber;

        SerializationService.CurrentProductionLogPartChanged += HandleProductionLogPartsChanged;
        SerializationService.CurrentProductNumberChanged += HandleProductNumberChanged;
        ProductionLogParts = SerializationService.CurrentProductionLogParts;

        IsLoading = false;
    }
    
    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {

            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                "./Components/Pages/ProductionLog/Create.razor.js");
        }
    }
    
    private async Task<List<ProductionLog>> LoadCachedFormsAsync()
    {
        var cachedFormsData = await LocalCacheManager.GetProductionLogBatchAsync();

        if (cachedFormsData == null || cachedFormsData.Count == 0)
        {
            AddProductionLogs(BatchSize);
            return ProductionLogBatch.Logs;
        }

        var productionLogs = cachedFormsData.Select(cachedForm => new ProductionLog
        {
            Id = cachedForm.ProductionLogId,
            LogSteps = cachedForm.LogSteps.Select(step => new ProductionLogStep
            {
                WorkInstructionStepId = step.WorkInstructionStepId,
                ProductionLogId = step.ProductionLogId,
                Attempts = step.Attempts.Select(a => new ProductionLogStepAttempt
                {
                    Success = a.Success,
                    Notes = a.Notes ?? "",
                    SubmitTime = a.SubmitTime
                }).ToList()
            }).ToList()
        }).ToList();

        // Setting status for each log or the whole batch
        foreach (var log in productionLogs)
        {
            if (log.LogSteps.All(step => step.Attempts.Any(a => a.SubmitTime != DateTimeOffset.MinValue)))
            {
                WorkInstructionStatus = Status.Completed;
            }
            else
            {
                WorkInstructionStatus = Status.InProgress;
            }
        }

        return productionLogs;
    }
    
    private async Task SetActiveWorkInstruction(int workInstructionId)
    {
        if (workInstructionId <= 0)
        {
            ActiveWorkInstruction = null;
            await SetSelectedWorkInstructionId(null);
            ProductionLogEventService.SetCurrentWorkInstructionName(string.Empty);
            return;
        }

        if (ActiveProductWorkInstructionList != null)
        {
            var workInstruction = await WorkInstructionService.GetByIdAsync(workInstructionId);
            if (workInstruction?.Products == null)
                return;

            // Reset the cached log and internal state
            await LocalCacheManager.ClearProductionLogBatchAsync();
            ProductionLogBatch.Logs.Clear();
            await ProductionLogEventService.SetCurrentProductionLogs(new List<ProductionLog>());
            AddProductionLogs(BatchSize);

            // Proceed with setting new state
            await SetSelectedWorkInstructionId(workInstructionId);
            ActiveWorkInstruction = workInstruction;
            ProductionLogEventService.SetCurrentWorkInstructionName(workInstruction.Title);
            await LocalCacheManager.SetActiveWorkInstructionIdAsync(workInstruction.Id);
        }
    }

    private async Task SetActiveProduct(int productId)
    {
        if (Products == null)
            return;

        if (productId < 0)
        {
            ActiveWorkInstruction = null;
            ActiveProductWorkInstructionList = null;
            await SetActiveWorkInstruction(-1);
            await LocalCacheManager.SetActiveProductAsync(null);
            return;
        }

        var product = Products.FirstOrDefault(p => p.Id == productId);
        if (product?.WorkInstructions == null)
            return;

        // Reset the cached log and internal state
        await LocalCacheManager.ClearProductionLogBatchAsync();
        ProductionLogBatch.Logs.Clear();
        await ProductionLogEventService.SetCurrentProductionLogs(new List<ProductionLog>());

        // Proceed with setting new state
        ActiveProduct = product;
        ActiveProductWorkInstructionList = product.WorkInstructions.Where(w => w.IsActive).ToList();
        ProductionLogEventService.SetCurrentProductName(product.Name);
        await SetActiveWorkInstruction(-1);
        await LocalCacheManager.SetActiveProductAsync(product);
    }

    
    private async Task GetCachedActiveProductAsync()
    {
        var result = await LocalCacheManager.GetActiveProductAsync();
        ActiveProduct = Products?.FirstOrDefault(p => p.Name == result.Name);

        if (ActiveProduct == null) 
        {
            return;
        }
        
        ActiveProductWorkInstructionList = ActiveProduct.WorkInstructions?
            .Where(w => w.IsActive)
            .ToList() ?? new List<WorkInstruction>();
        
        ProductionLogEventService.SetCurrentProductName(ActiveProduct.Name);
    }
    
    private Task OnBatchSizeChanged(int newSize)
    {
        if (newSize < 1)
            newSize = 1;

        BatchSize = newSize;

        int currentCount = ProductionLogBatch.Logs.Count;

        if (newSize > currentCount)
        {
            // Add new logs
            int logsToAdd = newSize - currentCount;
            AddProductionLogs(logsToAdd);
        }
        else if (newSize < currentCount)
        {
            // Remove excess logs
            ProductionLogBatch.Logs.RemoveRange(newSize, currentCount - newSize);
            ProductionLogEventService.SetCurrentProductionLogs(ProductionLogBatch.Logs);
        }

        return Task.CompletedTask;
    }

    /// Sets the local storage variable
    private async Task SetInProgressAsync(bool isActive)
    {
        await LocalCacheManager.SetIsWorkflowActiveAsync(isActive);
        IsWorkflowActive = isActive;
    }

    private async Task GetInProgressAsync()
    {
        var result = await LocalCacheManager.GetWorkflowActiveStatusAsync();
        
        // If the result is true, then the operator was previously in the middle of a workflow
        if (result)
        {
            IsWorkflowActive = result;
            await GetCachedActiveWorkInstructionAsync();
            await GetCachedActiveProductAsync();
            return;
        }

        await SetInProgressAsync(false);
    }
    

    private async Task GetCachedActiveWorkInstructionAsync()
    {
        var result = await LocalCacheManager.GetActiveWorkInstructionIdAsync(); 
        await LoadActiveWorkInstruction(result);
    }

    private async Task LoadProducts()
    {
        try
        {
            var productsAsync = await ProductService.GetAllAsync();
            Products = productsAsync.ToList();
        }
        catch (Exception e)
        {
            Log.Error("Error loading products for the Create view: {Message}", e.Message);
        }
    }
    
    private async Task SetSelectedWorkInstructionId(int? value)
    {
        if (value.HasValue)
        {
            await SetInProgressAsync(true);
        }
        await LocalCacheManager.SetActiveWorkInstructionIdAsync(value ?? -1);
    }
    
    private async Task LoadActiveWorkInstruction(int id)
    {
        ActiveWorkInstruction = await WorkInstructionService.GetByIdAsync(id);
        if (ActiveWorkInstruction == null)
        {
            return;
        }
        ProductionLogEventService.SetCurrentWorkInstructionName(ActiveWorkInstruction.Title);
    }

    /// <summary>
    /// Handles the submission of the production log. 
    /// Validates if all required parts have serial numbers before proceeding.
    /// If parts are missing serial numbers, prompts the user for confirmation.
    /// </summary>
    /// <remarks>
    /// - If `ActiveWorkInstruction` is null, the method exits early.
    /// - Calculates the total number of parts needed based on the active work instruction.
    /// - Compares the count of serial numbers logged with the total parts needed.
    /// - If all parts have serial numbers, proceeds to complete the submission.
    /// - Otherwise, displays a confirmation modal to the user.
    /// </remarks>
    /// <returns>An asynchronous operation.</returns>
    protected async Task HandleSubmit()
    {
        if (ActiveWorkInstruction == null)
        {
            return;
        }
        
        var partNodes = ActiveWorkInstruction.Nodes.Where(node => node.NodeType == WorkInstructionNodeType.Part);

        int totalPartsNeeded = 0;
        foreach (var node in partNodes)
        {
            // Cast to PartNode to access its Parts collection
            if (node is PartNode partNode)
            {
                totalPartsNeeded += partNode.Parts.Count;
            }
        }
        

        bool allStepsHavePartsNeeded = ProductionLogParts.Count >= totalPartsNeeded;

        if (!allStepsHavePartsNeeded)
        {
            popupRef?.Show("There are parts without serial numbers. Are you sure you want to submit this log?");
        }
        else
        {
            await CompleteSubmit();
        }
    }

    private async Task HandleConfirmation(bool confirmed)
    {
        if (confirmed)
        {
            await CompleteSubmit();
        }
    }

    private async Task CompleteSubmit()
    {
        var currentTime = DateTimeOffset.UtcNow;
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        foreach (var productionLog in ProductionLogEventService.CurrentProductionLogs)
        {

            // Update log
            productionLog.CreatedOn = currentTime;
            productionLog.LastModifiedOn = currentTime;
            productionLog.WorkInstruction = ActiveWorkInstruction;
            productionLog.Product = ActiveProduct;
            productionLog.OperatorId = userId;
            productionLog.FromBatchOf = BatchSize;

            // If no log is created, it gets created now to utilize the id for the QR code
            if (productionLog.Id <= 0)
            {
                var id = await ProductionLogService.CreateAsync(productionLog);
                productionLog.Id = id;
            }
            else
            {
                await ProductionLogService.UpdateAsync(productionLog);
            }

            if (ActiveWorkInstruction is { ShouldGenerateQrCode: true })
            {
                await PrintQRCode(productionLog.Id);
            }

            ToastService.ShowSuccess("Successfully Created Production Log", 3000);

            // Create any associated SerialNumberLogs
            await SerializationService.SaveCurrentProductionLogPartsAsync(productionLog.Id);
            
            // Add the new log to the session
            await SessionManager.AddProductionLogAsync(productionLog.Id);
        }

        // Reset the local storage values
        await LocalCacheManager.ClearProductionLogBatchAsync();
        
        await ResetFormState();
    }
    
    private void HandleProductionLogPartsChanged()
    {
        ProductionLogParts = SerializationService.CurrentProductionLogParts;
    
        InvokeAsync(StateHasChanged);
    }

    private void HandleProductNumberChanged()
    {
        ProductSerialNumber = SerializationService.CurrentProductNumber;

        InvokeAsync(StateHasChanged);
    }
    
    private void GenerateQRCode(int productionLogId)
    {
        var productionLogIdString = productionLogId + ",";
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(productionLogIdString, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrCodeData);
        var qrCodeImageArr = qrCode.GetGraphic(20);
    
        QRCodeDataUrl = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImageArr)}";
    }
    
    private async Task PrintQRCode(int productionLogId)
    {
        GenerateQRCode(productionLogId);
        if (string.IsNullOrEmpty(QRCodeDataUrl))
            return;
        
        if (module == null)
        {
            return;
        }
        await module.InvokeVoidAsync("printQRCode", QRCodeDataUrl);
    }

    private async Task ResetFormState()
    {
        // Reset the list to a new empty list
        ProductionLogBatch.Logs = new List<ProductionLog>();

        // Notify the event service with the empty list
        await ProductionLogEventService.SetCurrentProductionLogs(ProductionLogBatch.Logs);

        // Reinitialize the form with the current batch size
        AddProductionLogs(BatchSize);
        
        ProductionLogEventService.EnableAutoSave();
        WorkInstructionStatus = Status.NotStarted;
    }
    
    private async Task OnStepCompleted(List<ProductionLogStep> productionLogSteps, bool? success)
    {
        if (ActiveWorkInstruction == null)
            return;
        
        await ProductionLogEventService.SetCurrentProductionLogs(ProductionLogBatch.Logs);
            
        var currentStatus = await GetWorkInstructionStatus();
        WorkInstructionStatus = currentStatus ? Status.Completed : Status.InProgress;
        
        var step = productionLogSteps.FirstOrDefault();
        
        if (step == null)
            return;
        
        // Find the current node that corresponds to this step
        var currentNode = ActiveWorkInstruction.Nodes.FirstOrDefault(n => n.Id == step.WorkInstructionStepId);

        if (currentNode != null)
        {
            // Sort nodes by Position to maintain the correct sequence
            var orderedNodes = ActiveWorkInstruction.Nodes
                .OrderBy(n => n.Position)
                .ToList();

            var currentIndex = orderedNodes.FindIndex(n => n.Id == currentNode.Id);

            if (currentIndex >= 0 && currentIndex < orderedNodes.Count - 1)
            {
                var nextStep = orderedNodes[currentIndex + 1];

                // If a step completed successfully, scroll to the next work instruction node
                if (success == true)
                {
                    string elementId = $"step-{nextStep.Position}";

                    if (module != null)
                    {
                        await module.InvokeVoidAsync("scrollToStep", elementId);
                    }
                } 
            }
            
            // Scroll to the submit button if it's the last step and it was successful
            if (currentIndex == orderedNodes.Count - 1 && success == true)
            {
                if (module != null)
                {
                    await module.InvokeVoidAsync("scrollToStep", "submit-button");
                }
            }
        }

    }
    
    /// <summary>
    /// Determines if the operator has completed all steps in all production logs.
    /// </summary>
    /// <returns>Returns true if each step in every production log has been completed; false otherwise.</returns>
    private async Task<bool> GetWorkInstructionStatus()
    {
        try
        {
            return await Task.Run(() =>
            {
                // Check if every log's steps have attempts with valid submit times
                return ProductionLogBatch.Logs != null && ProductionLogBatch.Logs.All(log =>
                    log.LogSteps != null && log.LogSteps.All(step =>
                        step.Attempts.Any(a => a.SubmitTime != DateTimeOffset.MinValue)));
            });
        }
        catch (Exception e)
        {
            Log.Error("Error checking work instruction status: {Message}", e.Message);
            return false;
        }
    }
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        SerializationService.CurrentProductionLogPartChanged -= HandleProductionLogPartsChanged;
        SerializationService.CurrentProductNumberChanged -= HandleProductNumberChanged;
        ProductionLogEventService.AutoSaveTriggered -= _autoSaveHandler;
        try
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Deliberately not acting on the JSDisconnectedException since it is the preferred
            // way to handle disposed JS scripts without logging: https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-9.0
        }
        catch (JSException jsException)
        {
            Log.Warning("JS Interop Exception thrown, {Message}", jsException.Message);
        }
    }
    
    private void AddProductionLogs(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var emptyLog = new ProductionLog
            {
                LogSteps = ActiveWorkInstruction?.Nodes
                    .Where(n => n.NodeType == WorkInstructionNodeType.Step)
                    .Select(n => new ProductionLogStep
                    {
                        WorkInstructionStepId = n.Id,
                        Attempts = new List<ProductionLogStepAttempt>() // empty initially
                    })
                    .ToList() ?? new List<ProductionLogStep>()
            };

            ProductionLogBatch.Logs.Add(emptyLog);
        }

        // Notify downstream services
        ProductionLogEventService.SetCurrentProductionLogs(ProductionLogBatch.Logs);
    }
}