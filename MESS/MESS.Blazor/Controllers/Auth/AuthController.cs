using MESS.Services.ApplicationUser;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Blazor.Controllers.Auth;

[ApiController]
[Route("api/auth/")]
public class AuthController : ControllerBase
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IApplicationUserService applicationUserService, ILogger<AuthController> logger)
    {
        _applicationUserService = applicationUserService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string email)
    {
        try
        {
            var result = await _applicationUserService.SignInAsync(email);
            if (result)
            {
                _logger.LogInformation("User successfully logged in: {Email}", email);
                return Redirect("/");
            }
            
            _logger.LogInformation("Unsuccessful sign-in attempt");
            return Redirect("/auth/Login?error=true");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Redirect("/auth/Login?error=true");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _applicationUserService.SignOutAsync();
        return Redirect("/");
    }
}