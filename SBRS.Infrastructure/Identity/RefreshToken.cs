namespace SBRS.Infrastructure.Identity;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; }            // all tokens from one login share it
    public string TokenHash { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }      // null = never expires
    public DateTime? UsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}