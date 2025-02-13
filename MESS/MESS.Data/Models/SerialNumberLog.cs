namespace MESS.Data.Models;

public class SerialNumberLog : AuditableEntity
{
    public int Id { get; set; }
    public string? PartSerialNumber { get; set; }
    public string? ProductSerialNumber { get; set; }
    public int ProductionLogId { get; set; }
    public DateTimeOffset SubmitTimeQc { get; set; }
    
    // Navigation Field
    public Part? Part { get; set; }
}