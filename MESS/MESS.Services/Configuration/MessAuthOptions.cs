namespace MESS.Services.Configuration;

/// <summary>
/// Application-wide authentication behavior (username-only legacy vs password / SSO).
/// Bound from configuration section <c>MessAuth</c>.
/// </summary>
public sealed class MessAuthOptions
{
    /// <summary>Configuration section name in appsettings.</summary>
    public const string SectionName = "MessAuth";

    /// <summary>
    /// When true, users without a password may sign in with username only if their account still allows it.
    /// Set to false to require password or Microsoft sign-in for everyone.
    /// </summary>
    public bool AllowLegacyUsernameSignIn { get; set; } = true;
}
