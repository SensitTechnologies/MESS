using System.Security.Claims;
using System.Transactions;
using MESS.Data.Context;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace MESS.Services.ApplicationUser;
using Data.Models;
using Microsoft.EntityFrameworkCore;

/// <inheritdoc cref="IApplicationUserService"/>
public class ApplicationUserService : IApplicationUserService
{
    private readonly UserContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    const string DEFAULT_PASSWORD = "";
    const string DEFAULT_ROLE = "Operator";
    
    /// <inheritdoc cref="IApplicationUserService"/>
    public ApplicationUserService(UserContext context, UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <inheritdoc />
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
    
    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<List<ApplicationUser>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        var ApplicationUser = await _context.Users.FindAsync(id);
        return ApplicationUser;
    }

    /// <inheritdoc />
    public async Task<bool?> IsNewUser(ApplicationUser user)
    {
        try
        {
            if (string.IsNullOrEmpty(user.UserName) && string.IsNullOrEmpty(user.Email))
            {
                return null;
            }

            var existingUser = await _context.Users.FindAsync(user.Id);

            return existingUser == null;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to determine if user is new Source: {ExceptionSource}", e.Source);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByLastNameAsync(string lastName)
    {
        var ApplicationUser = await _context.Users.FirstOrDefaultAsync(n => n.LastName == lastName);
        return ApplicationUser;
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByUserNameAsync(string userName)
    {
        try
        {
            var ApplicationUser = await _context.Users.FirstOrDefaultAsync(n => n.UserName == userName);
            return ApplicationUser;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to find user with UserName: {Username}. Source: {ExceptionSource}", userName, e.Source);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        try
        {
            var ApplicationUser = await _context.Users.FirstOrDefaultAsync(n => n.Email == email);
            return ApplicationUser;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to find user with Email: {Email}. Source: {ExceptionSource}", email, e.Source);
            return null;
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<bool> UpdateApplicationUser(ApplicationUser applicationUser)
    {
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
        
            return await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
                var existingUser = await _context.Users.FirstOrDefaultAsync(
                    u => u.Id == applicationUser.Id);

                if (existingUser == null)
                {
                    Log.Error("Could not find user with ID {id}", applicationUser.Id);
                    return false;
                }

                applicationUser.NormalizedEmail = applicationUser.Email?.ToUpper();
                applicationUser.NormalizedUserName = applicationUser.UserName?.ToUpper();

                _context.Entry(existingUser).CurrentValues.SetValues(applicationUser);

                await _context.SaveChangesAsync();
                scope.Complete();

                Log.Information("Updated user with ID {id}", applicationUser.Id);
                return true;
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating user with ID {id}", applicationUser.Id);
            return false;
        }
    }

    /// <inheritdoc />
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
