using System.Security.Cryptography;
using System.Text;

namespace SBRS.Domain.Common;

public static class BeneficiaryMatchKey
{
    // All inputs must already be normalized.
    public static string Build(string firstName, string fatherName, string lastName,
        string motherName, DateOnly birthDate)
    {
        var raw = string.Join('|', firstName, fatherName, lastName, motherName,
            birthDate.ToString("yyyy-MM-dd"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
