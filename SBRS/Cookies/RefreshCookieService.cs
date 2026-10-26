using Microsoft.Extensions.Options;
using SBRS.Infrastructure.Identity;

namespace SBRS.Cookies;

public class RefreshCookieService(
    IHttpContextAccessor accessor,
    IOptions<RefreshCookieSettings> cookieOptions,
    IOptions<JwtSettings> jwtOptions)
    : IRefreshCookieService
{
    private readonly RefreshCookieSettings _settings = cookieOptions.Value;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public string? GetRefreshToken()
    {
        var value = accessor.HttpContext?.Request.Cookies[_settings.Name];
        return string.IsNullOrEmpty(value) ? null : value;
    }

    public void Set(string refreshToken)
    {
        // the cookie lives as long as the refresh token written into the database
        var expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);

        accessor.HttpContext?.Response.Cookies.Append(_settings.Name, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _settings.Secure,
            SameSite = ParseSameSite(_settings.SameSite),
            Path = _settings.Path,
            Domain = _settings.Domain,
            Expires = expires,
            IsEssential = true
        });
    }

    public void Delete()
    {
        accessor.HttpContext?.Response.Cookies.Delete(_settings.Name, new CookieOptions
        {
            Path = _settings.Path,
            Domain = _settings.Domain
        });
    }

    private static SameSiteMode ParseSameSite(string value) => value?.Trim().ToLowerInvariant() switch
    {
        "strict" => SameSiteMode.Strict,
        "none" => SameSiteMode.None,
        _ => SameSiteMode.Lax
    };
}
