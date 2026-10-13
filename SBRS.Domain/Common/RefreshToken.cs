namespace SBRS.Domain.Common;



public class RefreshToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string TokenHash { get; set; } = default!;
    public Guid FamilyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

