namespace MESS.Data.Models;

public class ProductStatus : AuditableEntity
{
    public int Id { get; set; }
    public required string StatusCode { get; set; }
}