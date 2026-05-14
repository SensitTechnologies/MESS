namespace MESS.Blazor.Configuration;

/// <summary>Reads Entra ID settings from flat App Service keys or AzureAd / AzureAd__ configuration.</summary>
public static class MicrosoftEntraAuthConfiguration
{
    /// <summary>Resolves credentials when all three values are present; otherwise clears outs and returns false.</summary>
    public static bool TryGetMicrosoftEntraCredentials(
        IConfiguration configuration,
        out string tenantId,
        out string clientId,
        out string clientSecret)
    {
        tenantId = FirstNonEmpty(configuration, "TenantId", "AzureAd:TenantId");
        clientId = FirstNonEmpty(configuration, "ClientId", "AzureAd:ClientId");
        clientSecret = FirstNonEmpty(configuration, "ClientSecret", "AzureAd:ClientSecret");

        if (string.IsNullOrWhiteSpace(tenantId)
            || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
        {
            tenantId = "";
            clientId = "";
            clientSecret = "";
            return false;
        }

        return true;
    }

    private static string FirstNonEmpty(IConfiguration configuration, string flatKey, string nestedKey)
    {
        var flat = configuration[flatKey];
        if (!string.IsNullOrWhiteSpace(flat))
            return flat;

        var nested = configuration[nestedKey];
        return nested ?? "";
    }
}
