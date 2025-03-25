using FluentValidation;

namespace MESS.Data.Models;

public class ProductionLogStep
{
    public int Id { get; set; }
    public int ProductionLogId { get; set; }
    public int WorkInstructionStepId { get; set; }
    public bool? Success { get; set; } = null;
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset SubmitTime { get; set; }
    
    public ProductionLog? ProductionLog { get; set; }
    public Step? WorkInstructionStep { get; set; }
}