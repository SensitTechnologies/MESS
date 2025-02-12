namespace MESS.Data.Models;

public class ProductionLog : AuditableEntity
{
    public int Id { get; set; }
    
    public DateTimeOffset SubmitTime { get; set; }
    
    // Navigation Fields
    public required LineOperator LineOperator { get; set; }
    public Product? Product { get; set; }
    public WorkStation? WorkStation { get; set; }
    public WorkInstruction? WorkInstruction { get; set; }
    public ProductStatus? ProductStatus { get; set; }
    public Problem? Problem { get; set; }
    public RootCause? RootCause { get; set; }
    public Cell? Cell { get; set; }
}