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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    const string DEFAULT_PASSWORD = "";
    const string DEFAULT_ROLE = "Operator";
    public ApplicationUserService(UserContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task SignOutAsync()
    {
        try
        {
            await _signInManager.SignOutAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task<bool> SignInAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null) await _signInManager.SignInAsync(user, isPersistent: false);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<List<ApplicationUser>> GetUsersByRoleAsync(string roleName)
    {
        try
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            return usersInRole.ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
    }

    public async Task<List<ApplicationUser>> GetApplicationUsers()
    {
        return await _context.Users.ToListAsync();
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
            var result = await _userManager.CreateAsync(ApplicationUser);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(ApplicationUser, DEFAULT_ROLE);
                Log.Information("Added ApplicationUser with ID {id}", ApplicationUser.Id);
                return true;
            }
            
            Log.Error("Unable to create ApplicationUser with ID {id}", ApplicationUser.Id);
            return false;
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
