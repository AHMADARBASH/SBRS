namespace SBRS.Cookies;

/// <summary>
/// The HttpOnly refresh-token cookie for web clients. Bound to config section
/// "RefreshCookie"; every value has a safe default, so the section may stay empty.
/// </summary>
public class RefreshCookieSettings
{
    public const string SectionName = "RefreshCookie";

    /// <summary>Cookie name; deleting must use the same name, path and domain.</summary>
    public string Name { get; set; } = "sbrs_rt";

    /// <summary>
    /// Limits the cookie to the auth endpoints. Must match the route casing exactly
    /// ("api/[controller]" → controller class name → "Auth"): browser cookie-path
    /// matching is case-sensitive, even though ASP.NET routing is not.
    /// </summary>
    public string Path { get; set; } = "/api/Auth";

    /// <summary>
    /// "Lax" (default) or "Strict". "None" requires Secure = true and is only
    /// needed when the web app and the API are on different registrable domains.
    /// </summary>
    public string SameSite { get; set; } = "Lax";

    public bool Secure { get; set; } = true;

    /// <summary>Leave null when the web app and the API share a registrable domain.</summary>
    public string? Domain { get; set; }
}
