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
public partial class Create : ComponentBase, IDisposable
{
    private string Title = "Add";
    private ConfirmationModal? popupRef;
    private bool IsWorkflowActive { get; set; }
    private Status WorkInstructionStatus { get; set; } = Status.NotStarted;
    private bool IsSaved { get; set; }
    
    private Product? ActiveProduct { get; set; }
    private WorkInstruction? ActiveWorkInstruction { get; set; }

    
    protected ProductionLog ProductionLog = new();
    
    private List<Product>? Products { get; set; }
    private List<WorkInstruction>? WorkInstructions { get; set; }
    
    private string? ActiveLineOperator { get; set; }
    private string? ProductSerialNumber { get; set; }
    private string? QRCodeDataUrl;
    private IJSObjectReference? module;
    private List<SerialNumberLog> _serialNumberLogs { get; set; } = [];
    
    private Func<ProductionLog, Task>? _autoSaveHandler;
    protected override async Task OnInitializedAsync()
    {
        ProductionLogEventService.DisableAutoSave();
        
        await LoadProducts();
        await LoadWorkInstructions();
        await GetInProgressAsync();

        var result = await AuthProvider.GetAuthenticationStateAsync();
        ActiveLineOperator = result.User.Identity?.Name;
        // This must come before the LoadCachedForm method since if it finds a cached form, it will set the status to InProgress
        WorkInstructionStatus = Status.NotStarted;
        
        var cachedForm = await LoadCachedForm();
        
        // Create ProductionLog in database since we will need the ID for QR codes
        if (!cachedForm)
        {
            var id = await ProductionLogService.CreateAsync(ProductionLog);
            ProductionLog.Id = id;
                
            ProductionLogEventService.EnableAutoSave();
        }
        
        // AutoSave Trigger
        _autoSaveHandler = async log =>
        {
            await LocalCacheManager.SetNewProductionLogFormAsync(log);
            await InvokeAsync((() =>
            {
                IsSaved = true;
                StateHasChanged();
            }));
        };
        
        ProductionLogEventService.AutoSaveTriggered += _autoSaveHandler;
        _serialNumberLogs = SerializationService.CurrentSerialNumberLogs;
        ProductSerialNumber = SerializationService.CurrentProductNumber;

        SerializationService.CurrentSerialNumberLogChanged += HandleSerialNumberLogsChanged;
        SerializationService.CurrentProductNumberChanged += HandleProductNumberChanged;

        await ProductionLogEventService.SetCurrentProductionLog(ProductionLog);
        
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                "./Components/Pages/ProductionLog/ProductionLogRadioButton.razor.js");
        }
    }
    
    private async Task<bool> LoadCachedForm()
    {
        var cachedFormData = await LocalCacheManager.GetProductionLogFormAsync();
        if (cachedFormData != null && cachedFormData.LogSteps.Count != 0)
        {
            WorkInstructionStatus = Status.InProgress;
            ProductionLog = new ProductionLog
            {
                Id = cachedFormData.ProductionLogId,
                LogSteps = cachedFormData.LogSteps.Select(step => new ProductionLogStep
                {
                    WorkInstructionStepId = step.WorkInstructionStepId,
                    Success = step.Success,
                    Notes = step.Notes ?? "",
                    SubmitTime = step.SubmitTime
                }).ToList()
            };
        }
        else
        {
            return false;
        }

        if (ProductionLog.LogSteps.TrueForAll(p => p.SubmitTime != DateTimeOffset.MinValue))
        {
            WorkInstructionStatus = Status.Completed;
        }

        return true;
    }
    
    
    private async Task SetActiveWorkInstruction(int workInstructionId)
    {
        if (workInstructionId <= 0)
        {
            ActiveWorkInstruction = null;
            await SetSelectedWorkInstructionId(null);
            ProductionLogEventService.SetCurrentWorkInstructionName(string.Empty);
        }
        if (WorkInstructions != null)
        {
            var workInstruction = WorkInstructions.FirstOrDefault(p => p.Id == workInstructionId);

            if (workInstruction?.Products == null)
            {
                return;
            }
            
            
            await SetSelectedWorkInstructionId(workInstructionId);

            ActiveWorkInstruction = workInstruction;
            ProductionLogEventService.SetCurrentWorkInstructionName(ActiveWorkInstruction.Title);
            
            await LocalCacheManager.SetActiveWorkInstructionIdAsync(workInstruction.Id);
        }
    }

    private async Task SetActiveProduct(int productId)
    {
        if (Products != null)
        {
            if (productId < 0)
            {
                ActiveWorkInstruction = null;
                return;
            }
            
            var product = Products.FirstOrDefault(p => p.Id == productId);

            if (product?.WorkInstructions == null)
            {
                return;
            }

            ActiveProduct = product;
            ProductionLogEventService.SetCurrentProductName(ActiveProduct.Name);
            await SetActiveWorkInstruction(-1);
            
            await LocalCacheManager.SetActiveProductAsync(product);
        }
    }
    
    private async Task GetCachedActiveProductAsync()
    {
        var result = await LocalCacheManager.GetActiveProductAsync();
        ActiveProduct = Products?.FirstOrDefault(p => p.Name == result.Name);

        if (ActiveProduct == null) 
        {
            return;
        }
        
        ProductionLogEventService.SetCurrentProductName(ActiveProduct.Name);
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
            var productsAsync = await ProductService.GetAllProductsAsync();
            Products = productsAsync.ToList();
        }
        catch (Exception e)
        {
            Log.Error("Error loading products for the Create view: {Message}", e.Message);
        }
    }
    
    
    private async Task LoadWorkInstructions()
    {
        try
        {
            var workInstructionsList = await WorkInstructionService.GetAllAsync();
            WorkInstructions = workInstructionsList.ToList();
        }
        catch (Exception e)
        {
            Log.Error("Error loading work instructions: {Message}", e.Message);
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

    protected async Task HandleSubmit()
    {
        if (ActiveWorkInstruction == null)
        {
            return;
        }
        
        int partsNeededCount = 0;
        ActiveWorkInstruction.Steps.ForEach(step => partsNeededCount += step.PartsNeeded?.Count ?? 0);

        bool allStepsHavePartsNeeded = _serialNumberLogs.Count >= partsNeededCount;

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
        await PrintQRCode();
        
        var currentTime = DateTimeOffset.UtcNow;
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Update log
        ProductionLog.CreatedOn = currentTime;
        ProductionLog.LastModifiedOn = currentTime;
        ProductionLog.WorkInstruction = ActiveWorkInstruction;
        ProductionLog.Product = ActiveProduct;
        ProductionLog.OperatorId = userId;
        await ProductionLogService.UpdateAsync(ProductionLog);
        
        // Create any associated SerialNumberLogs
        await SerializationService.SaveCurrentSerialNumberLogsAsync(ProductionLog.Id);
        
        // Reset the local storage values
        await LocalCacheManager.SetNewProductionLogFormAsync(null);

        
        // Add the new log to the session
        await SessionManager.AddProductionLogAsync(ProductionLog.Id);
        await ResetFormState();
    }
    
    private void HandleSerialNumberLogsChanged()
    {
        _serialNumberLogs = SerializationService.CurrentSerialNumberLogs;
    
        InvokeAsync(StateHasChanged);
    }

    private void HandleProductNumberChanged()
    {
        ProductSerialNumber = SerializationService.CurrentProductNumber;

        InvokeAsync(StateHasChanged);
    }
    
    private void GenerateQRCode()
    {
        var productionLogId = ProductionLogEventService.GetCurrentProductionLog()?.Id;
        var productionLogIdString = productionLogId + ",";
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(productionLogIdString, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrCodeData);
        var qrCodeImageArr = qrCode.GetGraphic(20);
    
        QRCodeDataUrl = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImageArr)}";
    }
    
    private async Task PrintQRCode()
    {
        GenerateQRCode();
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
        ProductionLog = new ProductionLog();
        var id = await ProductionLogService.CreateAsync(ProductionLog);
        ProductionLog.Id = id;
        await ProductionLogEventService.SetCurrentProductionLog(ProductionLog);
        ProductionLogEventService.EnableAutoSave();
        WorkInstructionStatus = Status.NotStarted;
    }
    
    private async Task OnStepCompleted(ProductionLogStep step, bool? success)
    {
        var currentTime = DateTimeOffset.UtcNow;
        step.SubmitTime = currentTime;
        step.Success = success;
        await ProductionLogEventService.SetCurrentProductionLog(ProductionLog);
        var currentStatus = await GetWorkInstructionStatus();
        WorkInstructionStatus = currentStatus ? Status.Completed : Status.InProgress;
    }
    
    /// <summary>
    /// Determines if the operator has completed all steps in the work instruction
    /// </summary>
    /// <returns>Returns True if each step in the work instruction has been completed. False otherwise.</returns>
    private async Task<bool> GetWorkInstructionStatus()
    {
        try
        {
            var result = false;
            await Task.Run(() =>
            {
                var t = ProductionLog.LogSteps.Find(s => s.SubmitTime == default);

                // If t is null then all steps have been completed
                if (t == null)
                {
                    result = true;
                }
            });

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Error("Error checking work instruction status: {Message}", e.Message);
            return false;
        }
    }
    public void Dispose()
    {
        ProductionLogEventService.AutoSaveTriggered -= _autoSaveHandler;
    }
    
    public async ValueTask DisposeAsync()
    {
        SerializationService.CurrentSerialNumberLogChanged -= HandleSerialNumberLogsChanged;
        if (module != null) await module.DisposeAsync();
    }
}