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
    public virtual DbSet<Step> Steps { get; set; } = null!;
    public virtual DbSet<WorkInstructionNode> WorkInstructionNodes { get; set; } = null!;
    public virtual DbSet<PartNode> PartNodes { get; set; } = null!;
    public virtual DbSet<SerialNumberLog> SerialNumberLogs { get; set; } = null!;
    public virtual DbSet<ProductionLog> ProductionLogs { get; set; } = null!;
    public virtual DbSet<Product> Products { get; set; } = null!;
    public virtual DbSet<Part> Parts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkInstructionNode>()
            .UseTptMappingStrategy();

        modelBuilder.Entity<PartNode>()
            .ToTable("PartNodes")
            .HasMany(p => p.Parts)
            .WithMany()
            .UsingEntity("PartNodeParts");

        modelBuilder.Entity<Step>()
            .ToTable("Steps");
        
        modelBuilder.Entity<ProductionLog>()
            .HasOne(p => p.WorkInstruction)
            .WithMany()
            .HasForeignKey("WorkInstructionId")
            .IsRequired(false);
    }

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