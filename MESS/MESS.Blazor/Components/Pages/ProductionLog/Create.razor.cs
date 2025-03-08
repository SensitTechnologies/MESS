using MESS.Services.ProductionLog;
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
public partial class Create : ComponentBase, IDisposable
{
    private string Title = "Add";
    private bool IsWorkflowActive { get; set; }
    private Status WorkInstructionStatus { get; set; } = Status.NotStarted;
    private bool IsSaved { get; set; }
    
    private Product? ActiveProduct { get; set; }
    private WorkStation? ActiveWorkStation { get; set; }
    private LineOperator? ActiveLineOperator { get; set; }
    
    protected ProductionLog ProductionLog = new();
    
    private List<WorkStation>? WorkStations { get; set; }
    private List<Product>? Products { get; set; }
    
    private WorkInstruction? ActiveWorkInstruction { get; set; }
    
    private Func<ProductionLog, Task>? _autoSaveHandler;
    protected override async Task OnInitializedAsync()
    {
        await LoadWorkStations();
        await LoadProducts();
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
        
        // TO BE REMOVED
        ActiveLineOperator = new LineOperator
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe"
        };
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
            LoadAssociatedProductsFromStation();
        }
    }

    private async Task SetActiveProduct(int productId)
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
        if (value.HasValue)
        {
            await LoadActiveWorkInstruction(value.Value);
            await SetCachedActiveWorkInstructionIdAsync(value.Value);
            await SetInProgressAsync(true);
        }
    }


    private async Task LoadActiveWorkInstruction(int id)
    {
        ActiveWorkInstruction = await WorkInstructionService.GetByIdAsync(id);
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
        ProductionLog.Product = ActiveProduct;
        ProductionLog.WorkStation = ActiveWorkStation;
        await ProductionLogService.CreateAsync(ProductionLog);
        
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

    private List<Product> LoadAssociatedProductsFromStation()
    {
        try
        {
            if (ActiveWorkStation == null || WorkStations == null)
            {
                return [];
            }

            return ActiveWorkStation.Products;
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