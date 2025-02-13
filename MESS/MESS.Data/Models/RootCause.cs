namespace MESS.Data.Models;

public class RootCause : AuditableEntity
{
    public int Id { get; set; }
    public string CauseCode { get; set; } = "";
    public bool IsActive { get; set; }
}