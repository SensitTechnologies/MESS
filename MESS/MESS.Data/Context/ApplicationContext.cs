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
    public virtual DbSet<LineOperator> LineOperators { get; set; } = null!;
    public virtual DbSet<Documentation> Documentations { get; set; } = null!;
    public virtual DbSet<Cell> Cells { get; set; } = null!;
}