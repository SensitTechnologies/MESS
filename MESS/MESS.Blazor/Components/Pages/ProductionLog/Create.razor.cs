using Microsoft.AspNetCore.Components;
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
    private bool IsWorkflowActive { get; set; }
    private Status WorkInstructionStatus { get; set; } = Status.NotStarted;
    private bool IsSaved { get; set; }
    
    private Product? ActiveProduct { get; set; }
    private WorkStation? ActiveWorkStation { get; set; }
    private WorkInstruction? ActiveWorkInstruction { get; set; }

    
    protected ProductionLog ProductionLog = new();
    
    private List<Product>? Products { get; set; }
    private List<WorkStation>? WorkStations { get; set; }
    private List<WorkInstruction>? WorkInstructions { get; set; }
    
    private string? ActiveLineOperator { get; set; }
    
    
    private Func<ProductionLog, Task>? _autoSaveHandler;
    protected override async Task OnInitializedAsync()
    {
        ProductionLogEventService.DisableAutoSave();
        
        await LoadWorkStations();
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
        

        await ProductionLogEventService.SetCurrentProductionLog(ProductionLog);
        
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
    
    private async Task SetActiveWorkStation(int workStationId)
    {
        if (WorkStations != null)
        {
            var workStation = WorkStations.FirstOrDefault(p => p.Id == workStationId);

            if (workStation?.Products == null)
            {
                return;
            }
            
            ActiveWorkStation = workStation;
            ProductionLogEventService.SetCurrentWorkStationName(ActiveWorkStation.Name);
            
            await LocalCacheManager.SetActiveWorkStationAsync(workStation);
        }
    }
    
    private async Task SetActiveWorkInstruction(int workInstructionId)
    {
        if (workInstructionId <= 0)
        {
            ActiveWorkInstruction = null;
            await SetSelectedWorkInstructionId(null);
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
            await GetCachedActiveWorkStationAsync();
            await GetCachedActiveWorkInstructionAsync();
            await GetCachedActiveProductAsync();
            return;
        }

        await SetInProgressAsync(false);
    }

    private async Task GetCachedActiveWorkStationAsync()
    {
        var result = await LocalCacheManager.GetActiveWorkStationAsync();
        ActiveWorkStation = WorkStations?.FirstOrDefault(w => w.Name == result.Name);
        
        if (ActiveWorkStation == null)
        {
            return;
        }
        
        ProductionLogEventService.SetCurrentWorkStationName(ActiveWorkStation.Name);
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
    
    private async Task LoadWorkStations()
    {
        try
        {
            var workStationsAsync = await WorkStationService.GetAllWorkStationsAsync();
            WorkStations = workStationsAsync.ToList();
        }
        catch (Exception e)
        {
            Log.Error("Error loading work stations for the Create view: {Message}", e.Message);
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
    
    private void LoadLineOperators()
    {
        try
        {
            var operatorsAsync = LineOperatorService.GetLineOperators();
            LineOperators = operatorsAsync.ToList();
        }
        catch (Exception e)
        {
            Log.Error("Error loading Line Operators for the Create view: {Message}", e.Message);
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
            Console.WriteLine("No Work Instruction selected.");
            return;
        }
        
        var currentTime = DateTimeOffset.UtcNow;
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Update log
        ProductionLog.CreatedOn = currentTime;
        ProductionLog.LastModifiedOn = currentTime;
        ProductionLog.WorkInstruction = ActiveWorkInstruction;
        ProductionLog.Product = ActiveProduct;
        ProductionLog.WorkStation = ActiveWorkStation;
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

    private List<WorkStation> LoadAssociatedWorkStationsFromProduct()
    {
        try
        {
            if (ActiveProduct == null || Products == null || ActiveProduct.WorkStations == null)
            {
                return [];
            }

            return ActiveProduct.WorkStations;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
    }

    /// <summary>
    /// Loads a list of work instructions based on the currently selected Product and Work Station
    /// </summary>
    /// <returns></returns>
    private List<WorkInstruction> LoadAssociatedWorkInstructions()
    {
        try
        {
            if (ActiveProduct?.WorkInstructions == null || Products?.Count <= 0 || 
                ActiveWorkStation?.WorkInstructions == null || WorkStations?.Count <= 0)
            {
                return [];
            }
            
            // WorkInstruction ID with a T/F for if they are within both lists
            var workInstructionMap = new Dictionary<int, bool>();
            
            foreach (var productWorkInstruction in ActiveProduct.WorkInstructions)
            {
                if (!workInstructionMap.TryAdd(productWorkInstruction.Id, false))
                {
                    workInstructionMap[productWorkInstruction.Id] = true;
                }
            }
            
            foreach (var stationWorkInstruction in ActiveWorkStation.WorkInstructions)
            {
                if (!workInstructionMap.TryAdd(stationWorkInstruction.Id, false))
                {
                    workInstructionMap[stationWorkInstruction.Id] = true;
                }
            }
            

            var commonWorkInstructionIds = workInstructionMap.Where(w => w.Value).Select(w => w.Key).ToList();

            var outputList = ActiveProduct.WorkInstructions
                .Where(w => commonWorkInstructionIds.Contains(w.Id)).ToList();

            if (outputList.Count > 0)
            {
                return outputList;
            }
                
            ActiveWorkInstruction = null;
            _ = SetActiveWorkInstruction(-1);

            return outputList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
    }
    public void Dispose()
    {
        ProductionLogEventService.AutoSaveTriggered -= _autoSaveHandler;
    }
}