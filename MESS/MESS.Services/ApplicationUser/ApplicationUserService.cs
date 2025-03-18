using System.Security.Claims;
using MESS.Data.Context;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace MESS.Services.ApplicationUser;
using Data.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationUserService : IApplicationUserService
{
    private readonly UserContext _context;
    public ApplicationUserService(UserContext context)
    {
        _context = context;
    }

    public List<ApplicationUser> GetApplicationUsers()
    {
        return _context.Users.ToList();
    }

    public ApplicationUser? GetApplicationUserById(string id)
    {
        var ApplicationUser = _context.Users.Find(id);
        return ApplicationUser;
    }

    public ApplicationUser? GetApplicationUserByLastName(string lastName)
    {
        var ApplicationUser = _context.Users.FirstOrDefault(n => n.LastName == lastName);
        return ApplicationUser;
    }

    public async Task<bool> AddApplicationUser(ApplicationUser ApplicationUser)
    {
        try
        {
            var ApplicationUserValidator = new ApplicationUserValidator();
            var validationResult = await ApplicationUserValidator.ValidateAsync(ApplicationUser);
        
            if (!validationResult.IsValid)
            {
                return false;
            }
        
            // await _context.ApplicationUsers.AddAsync(ApplicationUser);
            // await _context.SaveChangesAsync();
            Log.Information("Added ApplicationUser with ID {id}", ApplicationUser.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not add ApplicationUser");
            return false;
        }
    }

    public async Task<bool> UpdateApplicationUser(ApplicationUser ApplicationUser)
    {
        // var existingOperator = await _context.ApplicationUsers.FindAsync(ApplicationUser.Id);
        // if (existingOperator == null)
        // {
        //     Log.Error("Could not find ApplicationUser with ID {id}", ApplicationUser.Id);
        //     return false;
        // }
        //
        // existingOperator.FirstName = ApplicationUser.FirstName;
        // existingOperator.LastName = ApplicationUser.LastName;
        //
        await _context.SaveChangesAsync();
        // Log.Information("Updated ApplicationUser with ID {id}", ApplicationUser.Id);
        return true;
    }

    public async Task<bool> DeleteApplicationUser(string id)
    {
        var ApplicationUser = await _context.Users.FindAsync(id);
        
        if (ApplicationUser == null)
        {
            return false; 
        }
        
        // var relatedInstructions = await _context.WorkInstructions
            // .Where(w => w.Operator != null && w.Operator.Id == id)  
            // .ToListAsync();
        
        // if (relatedInstructions != null && relatedInstructions.Any())
        // {
            // foreach (var instruction in relatedInstructions)
            // {
                // instruction.Operator = null;  
            // }
        // }
        await _context.SaveChangesAsync(); 

        _context.Users.Remove(ApplicationUser);
        await _context.SaveChangesAsync(); 

        Log.Information("Successfully deleted ApplicationUser with ID {id}", ApplicationUser.Id);
        return true;
        }
    }
