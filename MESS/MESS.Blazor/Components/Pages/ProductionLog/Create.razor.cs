using Microsoft.AspNetCore.Components;
using Serilog;
namespace MESS.Blazor.Components.Pages.ProductionLog;

using Data.Models;

public partial class Create : ComponentBase
{
    [Parameter]
    public int? logId { get; set; }
    private bool IsEditMode => logId.HasValue;
    private string Title = "Add";
    private bool InProgress { get; set; }
    
    private string? ActiveProduct { get; set; }
    private string? ActiveWorkStation { get; set; }
    private string? ActiveLineOperator { get; set; }
    
    protected ProductionLog ProductionLog = new();
    
    private List<WorkStation>? WorkStations { get; set; }
    private List<Product>? Products { get; set; }
    
    private WorkInstruction? ActiveWorkInstruction { get; set; }
    private readonly ProductionLogValidator _validator = new();
    
    private Timer? _debounceTimer;
    private bool IsSaved { get; set; }

    private const int DELAY_TIME = 2000;

    protected override async Task OnInitializedAsync()
    {
        await GetInProgressAsync();
        await LoadWorkStations();
        await LoadProducts();
        await LoadProductionLog();
        
        
        var cachedFormData = await LocalCacheManager.GetProductionLogFormAsync();
        if (cachedFormData != null && cachedFormData.LogSteps.Any())
        {
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
        
        // TO BE REMOVED
        ActiveLineOperator = "John Doe";
    }
    
    private DateTimeOffset? GetLatestStepTime()
    {
        return ProductionLog.LogSteps
            .Where(s => s.SubmitTime != default)
            .Max(s => s.SubmitTime as DateTimeOffset?);
    }
    
    private async Task SetActiveWorkStation(int workStationId)
    {
        try
        {
            if (WorkStations != null)
            {
                var workStation = WorkStations.FirstOrDefault(p => p.Id == workStationId);

                if (workStation?.Products == null)
                {
                    return;
                }

                ActiveWorkStation = workStation.Name;
                
                await LocalCacheManager.SetActiveWorkStationAsync(workStation);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SetActiveProduct(int productId)
    {
        try
        {
            if (Products != null)
            {
                var product = Products.FirstOrDefault(p => p.Id == productId);

                if (product?.WorkInstructions == null)
                {
                    return;
                }

                // SETTING ACTIVE WORK INSTRUCTION TO THE FIRST IN THE LIST SINCE WE DO NOT YET KNOW IF THERE WILL BE 
                // AN ACTIVE WORK INSTRUCTION FOR EACH PRODUCT OR A WAY TO ALLOW OPERATORS TO CHOOSE
                await SetSelectedWorkInstructionId(int.Parse(product.WorkInstructions.First().Id.ToString()));
                ActiveProduct = product.Name;

                await LocalCacheManager.SetActiveProductAsync(product);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task GetCachedActiveProductAsync()
    {
        var result = await LocalCacheManager.GetActiveProductAsync();
        ActiveProduct = result.Name;
    }

    /// Sets the local storage variable
    private async Task SetInProgressAsync(bool inProgress)
    {
        try
        {
            await LocalCacheManager.SetInProgressAsync(inProgress);
            InProgress = inProgress;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// If retrieval fails set in progress too false to allow user to restart workflow
    private async Task GetInProgressAsync()
    {
        try
        {
            var result = await LocalCacheManager.GetInProgressAsync();
            
            // If the result is true, then the operator was previously in the middle of a workflow
            if (result)
            {
                InProgress = result;
                await GetCachedActiveWorkStationAsync();
                await GetCachedActiveWorkInstructionAsync();
                await GetCachedActiveProductAsync();
                
                return;
            }

            await SetInProgressAsync(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task GetCachedActiveWorkStationAsync()
    {
        try
        {
            var result = await LocalCacheManager.GetActiveWorkStationAsync();
            ActiveWorkStation = result.Name;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task GetCachedActiveWorkInstructionAsync()
    {
        try
        {
            var result = await LocalCacheManager.GetActiveWorkInstructionIdAsync(); 
            await LoadActiveWorkInstruction(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SetCachedActiveWorkInstructionIdAsync(int workInstructionId)
    {
        await LocalCacheManager.SetActiveWorkInstructionIdAsync(workInstructionId);
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
    
    private async Task LoadProductionLog()
    {
        if (logId.HasValue && logId.Value != 0)
        {
            await LoadExistingLog(logId.Value);
        }
    }
    
    
    private async Task SetSelectedWorkInstructionId(int? value)
    {
        try
        {
            if (value.HasValue)
            {
                await LoadActiveWorkInstruction(value.Value);
                await SetCachedActiveWorkInstructionIdAsync(value.Value);
                await SetInProgressAsync(true);
            }
        }
        catch (Exception e)
        {
            Log.Error("Error Setting selected work instruction ID: Exception: {e}", e.Message);
        }

    }


    private async Task LoadActiveWorkInstruction(int id)
    {
        ActiveWorkInstruction = await WorkInstructionService.GetByIdAsync(id);
    }

    /// Loads a Production Log from the database
    private async Task LoadExistingLog(int id)
    {
        var existingProductionLog = await ProductionLogService.GetByIdAsync(id);
        if (existingProductionLog != null)
        {
            Title = "Edit";
            ProductionLog = existingProductionLog;

            if (ProductionLog.WorkInstruction != null)
            {
                await SetSelectedWorkInstructionId(ProductionLog.WorkInstruction.Id);
            }

        }
        else
        {
            Console.WriteLine($"Production log with ID {id} not found.");
        }
    }

    protected async Task HandleSubmit()
    {
        if (ActiveWorkInstruction == null)
        {
            Console.WriteLine("No Work Instruction selected.");
            return;
        }

        var currentTime = DateTimeOffset.UtcNow;

        ProductionLog.WorkInstruction = ActiveWorkInstruction;

        if (IsEditMode)
        {
            // Update existing log
            ProductionLog.LastModifiedOn = currentTime;
            await ProductionLogService.UpdateAsync(ProductionLog);
            
            // Navigate to the log list page since there is no need to stay on the edit page since the state is unsaved
            NavigationManager.NavigateTo("/production-log");
        }
        else
        {
            // Create new log
            ProductionLog.CreatedOn = currentTime;
            ProductionLog.LastModifiedOn = currentTime;
            ProductionLog.SubmitTime = currentTime;
            await ProductionLogService.CreateAsync(ProductionLog);
        }

        // Reset the local storage values
        await LocalCacheManager.SetNewProductionLogFormAsync(null);
        NavigationManager.Refresh(true);
    }
    
    private void OnStepCompleted(ProductionLogStep step, bool success)
    {
        var currentTime = DateTimeOffset.UtcNow;
        
        // Only changing the submit time if it is the default value as to avoid issues where the operator
        // may have submitted the step and then changed their mind, which results in a negative time difference
        if (step.SubmitTime == DateTimeOffset.MinValue)
        {
            step.SubmitTime = currentTime;
        }
        step.Success = success;

        IsSaved = false;
        
        // Cancel current timer if it exists
        _debounceTimer?.DisposeAsync();
        
        _debounceTimer = new Timer(async _ =>
        {
            // Persist the changes to the local storage
            await LocalCacheManager.SetNewProductionLogFormAsync(ProductionLog);
            
            await InvokeAsync(() =>
            {
                IsSaved = true;
                StateHasChanged();
            });
        }, null, DELAY_TIME, Timeout.Infinite); // 2 Second delay

        StateHasChanged();
    }

    /// Removes the current 'state' from the browsers local storage since they are navigating to a page
    /// which will allow the operator to change product, or workstation, or to view previous session logs
    private async Task ResetCachedValues()
    {
        await LocalCacheManager.ResetCachedValuesAsync();
    }
}