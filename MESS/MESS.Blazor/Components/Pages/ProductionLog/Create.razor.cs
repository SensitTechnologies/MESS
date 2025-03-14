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
    private LineOperator? ActiveLineOperator { get; set; }
    private WorkInstruction? ActiveWorkInstruction { get; set; }

    
    protected ProductionLog ProductionLog = new();
    
    private List<Product>? Products { get; set; }
    private List<WorkStation>? WorkStations { get; set; }
    private List<WorkInstruction>? WorkInstructions { get; set; }
    private List<LineOperator>? LineOperators { get; set; }
    
    
    private Func<ProductionLog, Task>? _autoSaveHandler;
    protected override async Task OnInitializedAsync()
    {
        await LoadWorkStations();
        await LoadProducts();
        await LoadWorkInstructions();
        LoadLineOperators();
        await GetInProgressAsync();
        
        // This must come before the LoadCachedForm method since if it finds a cached form, it will set the status to InProgress
        WorkInstructionStatus = Status.NotStarted;
        
        await LoadCachedForm();
        
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
        
        if (ProductionLog != null)
        {
            await ProductionLogEventService.SetCurrentProductionLog(ProductionLog);
        }
    }
    
    private async Task LoadCachedForm()
    {
        var cachedFormData = await LocalCacheManager.GetProductionLogFormAsync();
        if (cachedFormData != null && cachedFormData.LogSteps.Count != 0)
        {
            WorkInstructionStatus = Status.InProgress;
            ProductionLog = new ProductionLog
            {
                LogSteps = cachedFormData.LogSteps.Select(step => new ProductionLogStep
                {
                    WorkInstructionStepId = step.WorkInstructionStepId,
                    Success = step.Success,
                    Notes = step.Notes ?? "",
                    SubmitTime = step.SubmitTime
                }).ToList()
            };
        }

        if (ProductionLog.LogSteps.TrueForAll(p => p.SubmitTime != DateTimeOffset.MinValue))
        {
            WorkInstructionStatus = Status.Completed;
        }
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
    
    private async Task SetActiveLineOperator(int lineOperatorId)
    {
        if (LineOperators != null)
        {
            var lineOperator = LineOperators.FirstOrDefault(p => p.Id == lineOperatorId);

            if (lineOperator == null)
            {
                return;
            }

            ActiveLineOperator = lineOperator;
            ProductionLogEventService.SetCurrentLineOperatorName(ActiveLineOperator.FullName);
            
            await LocalCacheManager.SetActiveLineOperatorAsync(lineOperator);
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
            LoadAssociatedWorkStationsFromProduct();
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
    
    private async Task GetCachedLineOperatorAsync()
    {
        var result = await LocalCacheManager.GetActiveLineOperatorAsync();
        ActiveLineOperator = LineOperators?.FirstOrDefault(p => p.FullName == result.Name);

        if (ActiveLineOperator == null) 
        {
            return;
        }
        
        ProductionLogEventService.SetCurrentLineOperatorName(ActiveLineOperator.FullName);
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
            await GetCachedLineOperatorAsync();
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
            Log.Error("Error loading products: {Message}", e.Message);
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
            Log.Error("Error loading work stations: {Message}", e.Message);
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
            Log.Error("Error loading products: {Message}", e.Message);
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
            Log.Error("Error loading products: {Message}", e.Message);
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
        
        // Update log
        ProductionLog.CreatedOn = currentTime;
        ProductionLog.LastModifiedOn = currentTime;
        ProductionLog.WorkInstruction = ActiveWorkInstruction;
        ProductionLog.Product = ActiveProduct;
        ProductionLog.WorkStation = ActiveWorkStation;
        ProductionLog.LineOperator = ActiveLineOperator;
        var productionLogId = await ProductionLogService.UpdateAsync(ProductionLog);
        
        // Create any associated SerialNumberLogs
        await SerializationService.SaveCurrentSerialNumberLogsAsync(ProductionLog.Id);
        
        // Reset the local storage values
        await LocalCacheManager.SetNewProductionLogFormAsync(null);
        await ProductionLogEventService.SetCurrentProductionLog(new ProductionLog());

        
        // Add the new log to the session
        await SessionManager.AddProductionLogAsync(ProductionLog.Id);
        ResetFormState();
    }

    private void ResetFormState()
    {
        ProductionLog = new ProductionLog();
        WorkInstructionStatus = Status.NotStarted;
        StateHasChanged();
    }
    
    private async Task OnStepCompleted(ProductionLogStep step, bool? success)
    {
        // Save log to database on first step complete
        if (ProductionLog.Id == 0)
        {
            if (step.Id == ProductionLog.LogSteps.First().Id)
            {
                var id = await ProductionLogService.CreateAsync(ProductionLog);
                ProductionLog.Id = id;
            }
        }
        var currentTime = DateTimeOffset.UtcNow;
        step.SubmitTime = currentTime;
        step.Success = success;
        IsSaved = false;
        await ProductionLogEventService.SetCurrentProductionLog(ProductionLog);
        var currentStatus = await GetWorkInstructionStatus();
        WorkInstructionStatus = currentStatus ? Status.Completed : Status.InProgress;

        StateHasChanged();
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

            return ActiveProduct.WorkInstructions
                .Where(w => commonWorkInstructionIds.Contains(w.Id)).ToList();

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