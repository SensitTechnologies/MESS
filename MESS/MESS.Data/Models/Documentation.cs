namespace MESS.Data.Models;

public class Documentation : AuditableEntity
{
    public int Id { get; set; }
    public required string Title { get; set; } = "";
    public string? ExternalLink { get; set; } = "";

    public required string ContentType { get; set; } = "";
    public required string Content { get; set; } = "";
    public string? FilePath { get; set; } = "";
    

}