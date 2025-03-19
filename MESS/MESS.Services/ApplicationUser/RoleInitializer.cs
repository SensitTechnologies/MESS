using Microsoft.AspNetCore.Identity;

namespace MESS.Services.ApplicationUser;

/// <summary>
/// Creates the associated roles within the Database via Microsoft Identity
/// </summary>
public class RoleInitializer
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleInitializer(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        string[] roles = { "Technician", "Operator" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}