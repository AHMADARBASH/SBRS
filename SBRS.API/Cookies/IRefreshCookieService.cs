namespace SBRS.Cookies;

/// <summary>
/// The single place that reads, writes and deletes the refresh-token cookie;
/// only AuthController uses it.
/// </summary>
public interface IRefreshCookieService
{
    /// <summary>The raw refresh token from the cookie, or null when missing or empty.</summary>
    string? GetRefreshToken();

    /// <summary>Writes the raw refresh token as an HttpOnly cookie.</summary>
    void Set(string refreshToken);

    /// <summary>Removes the cookie using the same name, path and domain used to write it.</summary>
    void Delete();
}
