namespace MESS.Data.Models;

public class ProductionLogStep
{
    public int Id { get; set; }
    public int ProductionLogId { get; set; }
    public int WorkInstructionStepId { get; set; }
    public bool Success { get; set; } = false;
    public string Notes { get; set; } = string.Empty;
    
    public ProductionLog? ProductionLog { get; set; }
    public Step? WorkInstructionStep { get; set; }
}