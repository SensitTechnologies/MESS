using System.Security.Claims;
using MESS.Blazor.Configuration;
using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.CRUD.ApplicationUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Blazor.Controllers.Auth;

/// <summary>
/// Handles login (username/password or legacy username-only), Microsoft 365 OIDC, and logout.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDbContextFactory<ApplicationContext> _dbFactory;
    private readonly bool _microsoftAuthEnabled;

    /// <summary>Injected dependencies.</summary>
    public AuthController(
        IApplicationUserService applicationUserService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IDbContextFactory<ApplicationContext> dbFactory,
        IConfiguration configuration)
    {
        _applicationUserService = applicationUserService;
        _signInManager = signInManager;
        _userManager = userManager;
        _dbFactory = dbFactory;
        _microsoftAuthEnabled = MicrosoftEntraAuthConfiguration.TryGetMicrosoftEntraCredentials(
            configuration,
            out _,
            out _,
            out _);
    }

    /// <summary>
    /// Username / password (or legacy username-only) login via form POST.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] string email, [FromForm] string? password)
    {
        try
        {
            var result = await _applicationUserService.SignInAsync(email, password);
            if (result)
            {
                Log.Information("User successfully logged in: {Email}", email);
                return Redirect("/production-log");
            }

            Log.Information("Unsuccessful sign-in attempt for {Email}", email);
            return Redirect("/auth/Login?failed=1");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Login failed for submitted identity {Email}", email);
            return Redirect("/auth/Login?failed=1");
        }
    }

    /// <summary>
    /// Starts Microsoft Entra ID / Microsoft 365 OpenID Connect sign-in.
    /// </summary>
    [HttpGet("microsoft")]
    [AllowAnonymous]
    public IActionResult MicrosoftLogin([FromQuery] string? returnUrl = null)
    {
        if (!_microsoftAuthEnabled)
        {
            return Redirect("/auth/Login?external=disabled");
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), values: new { returnUrl })!;
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectUrl);
        return Challenge(properties, "Microsoft");
    }

    /// <summary>
    /// OIDC callback: links external login to an existing MESS user (by email / UPN) or completes sign-in.
    /// </summary>
    [HttpGet("external-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback([FromQuery] string? returnUrl = null)
    {
        if (!_microsoftAuthEnabled)
        {
            return Redirect("/auth/Login?external=disabled");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            Log.Warning("External login callback with no login info");
            return Redirect("/auth/Login?external=noinfo");
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            var signedInUser = await _userManager.GetUserAsync(User);
            if (signedInUser != null)
            {
                await BackfillMessEmailFromMicrosoftClaimsAsync(signedInUser, info.Principal);
            }

            return LocalRedirect(SafeLocalReturnUrl(returnUrl));
        }

        if (signInResult.IsLockedOut)
        {
            return Redirect("/auth/Login?external=locked");
        }

        if (!GetMicrosoftIdentityCandidates(info.Principal).Any())
        {
            return Redirect("/auth/Login?external=noemail");
        }

        var user = await FindMessUserForExternalLoginAsync(info.Principal);
        if (user == null)
        {
            var tried = string.Join(", ", GetMicrosoftIdentityCandidates(info.Principal));
            Log.Information("Microsoft login: no local MESS user for identity candidates: {Candidates}", tried);
            var claimDump = string.Join(" | ", info.Principal.Claims.Select(c => $"{c.Type}={c.Value}"));
            Log.Debug("Microsoft login claim dump: {Claims}", claimDump);
            return Redirect("/auth/Login?external=nouser");
        }

        await BackfillMessEmailFromMicrosoftClaimsAsync(user, info.Principal);
        user = await _userManager.FindByIdAsync(user.Id) ?? user;

        if (user.LockoutEnd != null)
        {
            return Redirect("/auth/Login?external=locked");
        }

        var addResult = await _userManager.AddLoginAsync(user, info);
        if (!addResult.Succeeded)
        {
            Log.Warning("Failed to link Microsoft login for user {UserId}: {Errors}",
                user.Id, string.Join("; ", addResult.Errors.Select(e => e.Description)));
            return Redirect("/auth/Login?external=linkfailed");
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(SafeLocalReturnUrl(returnUrl));
    }

    /// <summary>
    /// Collects likely identity strings from Microsoft Entra ID tokens (v2 endpoint).
    /// </summary>
    private static IEnumerable<string> GetMicrosoftIdentityCandidates(ClaimsPrincipal principal)
    {
        void add(HashSet<string> set, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var t = value.Trim();
            if (t.Length > 0)
            {
                set.Add(t);
            }
        }

        var distinct = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        add(distinct, principal.FindFirstValue(ClaimTypes.Email));
        add(distinct, principal.FindFirstValue("preferred_username"));
        add(distinct, principal.FindFirstValue(ClaimTypes.Upn));
        add(distinct, principal.FindFirstValue("unique_name"));
        add(distinct, principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"));
        add(distinct, principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"));
        return distinct;
    }

    /// <summary>
    /// Resolves an existing MESS user from Microsoft claims (email, UPN, username),
    /// then falls back to a direct database match for legacy rows where normalized columns may be wrong.
    /// </summary>
    private async Task<ApplicationUser?> FindMessUserForExternalLoginAsync(ClaimsPrincipal principal)
    {
        var candidates = GetMicrosoftIdentityCandidates(principal).ToList();
        var expanded = new HashSet<string>(candidates, StringComparer.OrdinalIgnoreCase);
        foreach (var c in candidates.Where(x => x.Contains('@', StringComparison.Ordinal)))
        {
            var parts = c.Split('@', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                expanded.Add(parts[0].Trim());
            }
        }

        foreach (var c in expanded)
        {
            var byEmail = await _userManager.FindByEmailAsync(c);
            if (byEmail != null)
            {
                return byEmail;
            }

            var byName = await _userManager.FindByNameAsync(c);
            if (byName != null)
            {
                return byName;
            }
        }

        return await FindMessUserInDatabaseAsync(expanded);
    }

    /// <summary>
    /// Matches Microsoft identity strings against <c>AspNetUsers</c> using normalized columns
    /// and case-insensitive raw email / username columns (legacy data).
    /// </summary>
    private async Task<ApplicationUser?> FindMessUserInDatabaseAsync(IEnumerable<string> expanded)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        foreach (var c in expanded)
        {
            if (string.IsNullOrWhiteSpace(c))
            {
                continue;
            }

            var trimmed = c.Trim();
            var ne = _userManager.NormalizeEmail(trimmed);
            var nu = _userManager.NormalizeName(trimmed);
            var lower = trimmed.ToLowerInvariant();

            var row = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(u =>
                (u.NormalizedEmail != null && u.NormalizedEmail == ne)
                || (u.NormalizedUserName != null && u.NormalizedUserName == nu)
                || (u.Email != null && u.Email.ToLower() == lower)
                || (u.UserName != null && u.UserName.ToLower() == lower));

            if (row != null)
            {
                Log.Information(
                    "Microsoft login matched MESS user {UserId} via database fallback (candidate {Candidate})",
                    row.Id,
                    trimmed);
                return await _userManager.FindByIdAsync(row.Id);
            }
        }

        return null;
    }

    /// <summary>
    /// Picks a single email-style string (contains @) from Microsoft token claims for profile sync.
    /// </summary>
    private static string? PickEmailLikeFromMicrosoftClaims(ClaimsPrincipal principal)
    {
        foreach (var v in new[]
                 {
                     principal.FindFirstValue(ClaimTypes.Email),
                     principal.FindFirstValue("preferred_username"),
                     principal.FindFirstValue(ClaimTypes.Upn),
                 })
        {
            if (!string.IsNullOrWhiteSpace(v) && v.Contains('@', StringComparison.Ordinal))
            {
                return v.Trim();
            }
        }

        foreach (var c in GetMicrosoftIdentityCandidates(principal))
        {
            if (c.Contains('@', StringComparison.Ordinal))
            {
                return c;
            }
        }

        return null;
    }

    /// <summary>
    /// If the MESS user has no email, set it from Microsoft claims and mark confirmed (trusted IdP).
    /// </summary>
    private async Task BackfillMessEmailFromMicrosoftClaimsAsync(ApplicationUser user, ClaimsPrincipal principal)
    {
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var email = PickEmailLikeFromMicrosoftClaims(principal);
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var fresh = await _userManager.FindByIdAsync(user.Id);
        if (fresh is null || !string.IsNullOrWhiteSpace(fresh.Email))
        {
            return;
        }

        var setResult = await _userManager.SetEmailAsync(fresh, email);
        if (!setResult.Succeeded)
        {
            Log.Warning(
                "Microsoft login: could not backfill Email for user {UserId}: {Errors}",
                fresh.Id,
                string.Join("; ", setResult.Errors.Select(e => e.Description)));
            return;
        }

        var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(fresh);
        var confirmResult = await _userManager.ConfirmEmailAsync(fresh, confirmToken);
        if (!confirmResult.Succeeded)
        {
            Log.Warning(
                "Microsoft login: Email set but confirmation failed for user {UserId}: {Errors}",
                fresh.Id,
                string.Join("; ", confirmResult.Errors.Select(e => e.Description)));
            return;
        }

        Log.Information("Microsoft login: backfilled and confirmed Email for MESS user {UserId}", fresh.Id);
    }

    private string SafeLocalReturnUrl(string? returnUrl)
    {
        const string fallback = "/production-log";
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return fallback;
        }

        return Url.IsLocalUrl(returnUrl) ? returnUrl : fallback;
    }

    /// <summary>Logs out the current user.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await _applicationUserService.SignOutAsync();
        return Redirect("/");
    }
}
