namespace MESS.Data.Models;

public class Cell : AuditableEntity
{
    public int Id { get; set; }
    public string CellCode { get; set; } = "";
    public bool IsActive { get; set; }
}