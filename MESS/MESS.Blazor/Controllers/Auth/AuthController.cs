using MESS.Services.ApplicationUser;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MESS.Blazor.Controllers.Auth;

[ApiController]
[Route("api/auth/")]
public class AuthController : ControllerBase
{
    private readonly IApplicationUserService _applicationUserService;

    public AuthController(IApplicationUserService applicationUserService)
    {
        _applicationUserService = applicationUserService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string email)
    {
        try
        {
            var result = await _applicationUserService.SignInAsync(email);
            if (result)
            {
                Log.Information("User successfully logged in: {Email}", email);
                return Redirect("/");
            }
            
            Log.Information("Unsuccessful sign-in attempt");
            return Redirect("/auth/Login");
        }
        catch (Exception ex)
        {
            Log.Warning("Login failed: {Message}", ex.Message);
            return Redirect("/auth/Login");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _applicationUserService.SignOutAsync();
        return Redirect("/");
    }
}