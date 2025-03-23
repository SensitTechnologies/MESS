using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MESS.Data.Context;

public class ApplicationContext : DbContext
{
    public ApplicationContext()
    {
        
    }
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
        
    }

    public virtual DbSet<WorkInstruction> WorkInstructions { get; set; } = null!;
    public virtual DbSet<WorkStation> WorkStations { get; set; } = null!;
    public virtual DbSet<Step> Steps { get; set; } = null!;
    public virtual DbSet<SerialNumberLog> SerialNumberLogs { get; set; } = null!;
    public virtual DbSet<RootCause> RootCauses { get; set; } = null!;
    public virtual DbSet<ProductStatus> ProductStatus { get; set; } = null!;
    public virtual DbSet<ProductionLog> ProductionLogs { get; set; } = null!;
    public virtual DbSet<Product> Products { get; set; } = null!;
    public virtual DbSet<Problem> Problems { get; set; } = null!;
    public virtual DbSet<Part> Parts { get; set; } = null!;

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                // To be modified when User logic is added
                // entry.Entity.CreatedBy = "TheCreateUser";
                entry.Entity.CreatedOn = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                // To be modified when User logic is added
                // entry.Entity.LastModifiedBy = "TheUpdateUser";
                entry.Entity.LastModifiedOn = DateTime.UtcNow;
            }
        }
    }

}