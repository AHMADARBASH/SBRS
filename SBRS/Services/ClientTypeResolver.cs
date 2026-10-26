namespace SBRS.Services;

/// <summary>
/// Web clients send the header "X-Client-Type: web"; everything else (missing,
/// "mobile" or any other value) is treated as mobile, which keeps the previous
/// behavior for existing and unknown clients.
/// </summary>
public interface IClientTypeResolver
{
    bool IsWeb { get; }
}

/// <summary>
/// Requiring the custom header also forces a CORS preflight for cross-origin
/// web apps; browsers cannot attach custom headers to plain cross-site form
/// posts, so the header doubles as CSRF protection for the cookie flow.
/// </summary>
public class ClientTypeResolver(IHttpContextAccessor accessor) : IClientTypeResolver
{
    public bool IsWeb => string.Equals(
        accessor.HttpContext?.Request.Headers["X-Client-Type"].ToString(),
        "web", StringComparison.OrdinalIgnoreCase);
}
