namespace MESS.Data.DTO;

public class ProductionLogStepDTO
{
    public int WorkInstructionStepId { get; set; }
    public int ProductionLogId { get; set; }
    public bool? Success { get; set; }
    public DateTimeOffset SubmitTime { get; set; }
    public bool ShowNotes { get; set; }
    public string? Notes { get; set; }
}