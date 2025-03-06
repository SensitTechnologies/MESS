using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;
namespace MESS.Blazor.Components.Pages.ProductionLog;

using Data.Models;

internal enum Status
{
    NotStarted,
    InProgress,
    Completed
}
public partial class Create : ComponentBase
{
    private string Title = "Add";
    private bool IsWorkflowActive { get; set; }
    private Status WorkInstructionStatus { get; set; } = Status.NotStarted;
    
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
        
        // This must come before the LoadCachedForm method since if it finds a cached form, it will set the status to InProgress
        WorkInstructionStatus = Status.NotStarted;
        
        await LoadCachedForm();
        
        // TO BE REMOVED
        ActiveLineOperator = "John Doe";
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
    
    private async Task HandleResetConfiguration()
    {
        // Do not allow operator to reset configuration if work instruction is started or if it is deemed complete
        if (WorkInstructionStatus is Status.InProgress or Status.Completed)
        {
            return;
        }
        
        // Reset current form selection
        ActiveProduct = null;
        ActiveWorkStation = null;
        WorkInstructionStatus = Status.NotStarted;
        IsWorkflowActive = false;
        
        await ResetCachedValues();
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
                LoadAssociatedProductsFromStation();
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
    private async Task SetInProgressAsync(bool isActive)
    {
        try
        {
            await LocalCacheManager.SetIsWorkflowActiveAsync(isActive);
            IsWorkflowActive = isActive;
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
                ActiveWorkInstruction = ProductionLog.WorkInstruction;
            }

            if (ProductionLog.Product != null)
            {
                ActiveProduct = ProductionLog.Product.Name;
            }
            
            if (ProductionLog.WorkStation != null)
            {
                ActiveWorkStation = ProductionLog.WorkStation.Name;
            }

            StateHasChanged();
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
        
        // Create new log
        ProductionLog.CreatedOn = currentTime;
        ProductionLog.LastModifiedOn = currentTime;
        ProductionLog.WorkInstruction = ActiveWorkInstruction;
        ProductionLog.Product = Products?.Find(w => w.Name == ActiveProduct);
        ProductionLog.WorkStation = WorkStations?.Find(w => w.Name == ActiveWorkStation);
        await ProductionLogService.CreateAsync(ProductionLog);
        
        // Reset the local storage values
        await LocalCacheManager.SetNewProductionLogFormAsync(null);
        
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
        var currentTime = DateTimeOffset.UtcNow;
        
        step.SubmitTime = currentTime;
        
        step.Success = success;

        IsSaved = false;

        var currentStatus = await GetWorkInstructionStatus();

        WorkInstructionStatus = currentStatus ? Status.Completed : Status.InProgress;
        
        
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

    private List<Product> LoadAssociatedProductsFromStation()
    {
        try
        {
            if (ActiveWorkStation == null || WorkStations == null)
            {
                return [];
            }

            var workStation = WorkStations.FirstOrDefault(w => w.Name == ActiveWorkStation);

            if (workStation == null)
            {
                return [];
            }

            return workStation.Products;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
    }

    /// Removes the current 'state' from the browsers local storage since they are navigating to a page
    /// which will allow the operator to change product, or workstation, or to view previous session logs
    private async Task ResetCachedValues()
    {
        await LocalCacheManager.ResetCachedValuesAsync();
    }
}