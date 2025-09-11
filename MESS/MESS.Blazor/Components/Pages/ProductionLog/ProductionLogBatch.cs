using System.ComponentModel.DataAnnotations;
using MESS.Services.DTOs.ProductionLogs.Form;

namespace MESS.Blazor.Components.Pages.ProductionLog;

/// <summary>
/// Represents a temporary, UI-only collection of production log form DTOs
/// while the operator is creating or editing them in batch mode on the Blazor page.
/// </summary>
/// <remarks>
/// This class is used only in the Blazor UI layer to capture operator input.  
/// It is not persisted to the database and is not part of the service DTOs.  
/// Once the batch is submitted, its contents are passed to the service layer  
/// for processing, where they may be converted into create or update requests.  
/// </remarks>
public class ProductionLogBatch
{
    /// <summary>
    /// Gets or sets the list of production log form DTOs in the batch.  
    /// These remain in-memory only until the operator submits the batch.  
    /// </summary>
    [Required]
    public List<ProductionLogFormDTO> Logs { get; set; } = [];
}