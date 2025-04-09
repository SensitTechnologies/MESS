using MESS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MESS.Data.Seed;

/// <summary>
/// Creates a default user with Technician permissions to account for Database wipes.
/// </summary>
public static class InitialUserSeed
{
    public static async Task SeedDefaultUserAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "technician@mess.com";

        // Check if user already exists
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Technician");
            }
        }
    }
}