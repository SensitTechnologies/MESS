using System.Security.Claims;
using System.Transactions;
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

    public async Task<IdentityResult> AddApplicationUser(ApplicationUser ApplicationUser)
    {
        try
        {
            var result = await _userManager.CreateAsync(ApplicationUser);
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(ApplicationUser, DEFAULT_ROLE);
                Log.Information("Added ApplicationUser with ID {id}", ApplicationUser.Id);
                return IdentityResult.Success;
            }
            
            Log.Error("Unable to create ApplicationUser with ID {id}", ApplicationUser.Id);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not add ApplicationUser");
            return IdentityResult.Failed();
        }
    }

    public async Task<bool> UpdateApplicationUser(ApplicationUser applicationUser)
    {
        try
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
            var existingUser = await _context.Users.FindAsync(applicationUser.Id);
            if (existingUser == null)
            {
                Log.Error("Could not find user with ID {id}", applicationUser.Id);
                return false;
            }

            // Detach any entities so that they will not be saved on accident
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }

            _context.Users.Attach(existingUser);

            existingUser.FirstName = applicationUser.FirstName;
            existingUser.LastName = applicationUser.LastName;
            existingUser.Email = applicationUser.Email;
            existingUser.UserName = applicationUser.UserName;

            _context.Entry(existingUser).State = EntityState.Modified;
        
            await _context.SaveChangesAsync();
            scope.Complete();

            Log.Information("Updated user with ID {id}", applicationUser.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating user with ID {id}", applicationUser.Id);
            return false;
        }
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
