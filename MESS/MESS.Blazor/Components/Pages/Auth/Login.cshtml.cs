using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MESS.Data.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;

namespace MESS.Blazor.Components.Pages.Auth
{
    public class Login : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty] public string Email { get; set; } = "";
        
        public Login(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Email);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    Log.Information("User with ID successfully logged in: {ID}", user.Id);
                    return RedirectToPage("/production-log");
                }
                else
                {
                    Log.Information("User not found");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return Page();
        }
    }
}