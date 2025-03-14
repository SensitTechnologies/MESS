namespace MESS.Data.Models;

public class Part : AuditableEntity
{
    public int Id { get; set; }
    public required string PartNumber { get; set; }
    public required string PartName { get; set; }
    public bool IsActive { get; set; }
}