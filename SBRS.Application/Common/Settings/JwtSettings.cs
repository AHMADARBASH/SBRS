
namespace SBRS.Application.Common.Settings;
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Key { get; set; } = default!;
    public int AccessTokenExpiryMinutes { get; set; } = 240;
    public int RefreshTokenExpiryDays { get; set; } = 30;
}