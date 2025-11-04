using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.CRUD.PartDefinitions;

/// <inheritdoc />
public class PartDefinitionService : IPartDefinitionService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartDefinitionService"/> class.
    /// </summary>
    /// <param name="contextFactory">
    /// The <see cref="IDbContextFactory{TContext}"/> used to create instances of the
    /// <see cref="ApplicationContext"/> for database operations.
    /// </param>
    /// <remarks>
    /// This constructor sets up the service for performing create-or-retrieve operations
    /// on <see cref="PartDefinition"/> entities. The provided <paramref name="contextFactory"/>
    /// ensures that each operation uses its own DbContext instance, improving performance
    /// and avoiding tracking conflicts in multithreaded or scoped environments.
    /// </remarks>
    public PartDefinitionService(
        IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    /// <inheritdoc />
    public async Task<PartDefinition?> GetOrAddPartAsync(PartDefinition partDefinitionToAdd)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingPart = await context.PartDefinitions.FirstOrDefaultAsync(p =>
                p.Name == partDefinitionToAdd.Name &&
                p.Number == partDefinitionToAdd.Number);


            if (existingPart != null)
            {
                // Detach from context so that EF Core does not attempt to re-add it to the database
                context.Entry(existingPart).State = EntityState.Unchanged;
                Log.Information("GetOrAddPart: Successfully found pre-existing Part with ID: {ExistingPartID}", existingPart.Id);
                return existingPart;
            }
            
            // If a part does not exist in the database create it here
            // and return with database generated ID
            await context.PartDefinitions.AddAsync(partDefinitionToAdd);
            await context.SaveChangesAsync();
            Log.Information("Successfully created a new Part with Name: {PartName}, and Number: {PartNumber}", partDefinitionToAdd.Name, partDefinitionToAdd.Number);
            return partDefinitionToAdd;
        }
        catch (Exception e)
        {
            Log.Warning("Exception when adding part: {Exception}", e.ToString());
            return null;
        }
    }
}