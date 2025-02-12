namespace MESS.Data.Models;

public class LineOperator : AuditableEntity
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public int? ProductionLogId { get; set; }
    public ProductionLog? ProductionLog { get; set; }
}