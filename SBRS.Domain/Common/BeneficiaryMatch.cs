using SBRS.Domain.Entities;
using SBRS.Domain.Enums;

namespace SBRS.Domain.Common;

public sealed record BeneficiaryMatch(
    BeneficiaryEntity Beneficiary,
    BeneficiaryMatchStatus Status,
    int Score,
    BeneficiaryMatchReasons Reasons);
