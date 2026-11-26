using SBRS.Application.Common.Validation;
using SBRS.Application.Features.Beneficiaries.Commands;
using SBRS.Application.Features.BeneficiaryProfiles.Commands;
using SBRS.Domain.Constants;
using Xunit;

namespace SBRS.Tests.Validation;

public class BeneficiaryFormRulesTests
{
    [Theory]
    [InlineData("Ali", true)]
    [InlineData("  Ali  ", true)]
    [InlineData("محمد", true)]
    [InlineData("محمد علي", true)]
    [InlineData("محمدٌ", true)]                 // Arabic diacritic
    [InlineData("Jean Pierre", true)]
    [InlineData("A", false)]                    // one character
    [InlineData("محمد  علي", false)]            // double space
    [InlineData("Ali1", false)]                 // Latin digit
    [InlineData("محمد١", false)]                // Arabic-Indic digit
    [InlineData("علی۲", false)]                 // Persian digit
    [InlineData("Ali-Hassan", false)]           // hyphen
    [InlineData("Ali.Hassan", false)]           // dot
    [InlineData("Ali!", false)]                 // symbol
    public void PersonName_IsValidated(string value, bool expected) =>
        Assert.Equal(expected, BeneficiaryFormRules.IsValidPersonName(value));

    [Theory]
    [InlineData("Ali Hassan", true)]
    [InlineData("  Ali Hassan ", true)]
    [InlineData("", true)]                      // optional
    [InlineData("   ", true)]                   // optional
    [InlineData("A", false)]
    [InlineData("Ali 2", false)]
    [InlineData("Ali, Hassan", false)]
    public void WorkText_IsValidated(string value, bool expected) =>
        Assert.Equal(expected, BeneficiaryFormRules.IsValidWorkText(value));

    [Theory]
    [InlineData("0912345678", true, "0912345678")]          // local mobile
    [InlineData("+963912345678", true, "0912345678")]       // +963 mobile
    [InlineData("00963912345678", true, "0912345678")]      // 00963 mobile
    [InlineData("963912345678", true, "0912345678")]        // 963 mobile
    [InlineData("٠٩١٢٣٤٥٦٧٨", true, "0912345678")]          // Arabic-Indic digits
    [InlineData("0912-345 678", true, "0912345678")]        // spaces and dashes removed
    [InlineData("011123456", true, "011123456")]            // landline, 6-digit subscriber
    [InlineData("0111234567", true, "0111234567")]          // landline, 7-digit subscriber
    [InlineData("+96311123456", true, "011123456")]         // landline with country code
    [InlineData("+963111234567", true, "0111234567")]       // landline with country code, long
    [InlineData("091123456", false, null)]                  // landline governorate code may not start with 9
    [InlineData("09123456789", false, null)]                // 11 digits
    [InlineData("0912ABC678", false, null)]                 // letters
    [InlineData("+971912345678", false, null)]              // foreign country code
    [InlineData("+9630912345678", false, null)]             // country code plus local leading 0
    public void Phone_IsNormalizedToLocalFormat(string value, bool valid, string? canonical)
    {
        var result = BeneficiaryFormRules.TryNormalizePhone(value, out var normalized);
        Assert.Equal(valid, result);
        Assert.Equal(canonical, normalized);
    }

    [Theory]
    [InlineData(1990, 5, 4, false, true)]
    [InlineData(1990, 5, 4, true, true)]
    [InlineData(1899, 12, 31, false, false)]
    [InlineData(1899, 12, 31, true, false)]
    [InlineData(2999, 1, 1, false, false)]
    public void BirthDate_IsValidated(int year, int month, int day, bool estimated, bool expected) =>
        Assert.Equal(expected, BeneficiaryFormRules.IsValidBirthDate(new DateOnly(year, month, day), estimated));

    [Fact]
    public void EnrollmentDate_CannotBeBeforeBirthOrInTheFuture()
    {
        var birth = new DateOnly(1990, 5, 4);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        Assert.True(BeneficiaryFormRules.IsValidEnrollmentDate(birth, today, false));
        Assert.False(BeneficiaryFormRules.IsValidEnrollmentDate(birth, birth.AddDays(-1), false));
        Assert.False(BeneficiaryFormRules.IsValidEnrollmentDate(birth, today.AddDays(1), false));
    }

    [Theory]
    [InlineData(true, true, false)]    // displaced, no date, no area -> invalid
    [InlineData(true, false, true)]    // displaced, date, no area -> invalid
    [InlineData(false, false, true)]   // not displaced, date sent -> invalid
    [InlineData(true, false, false)]   // displaced, no date, area -> invalid
    public void Displaced_ConditionalRules(bool isDisplaced, bool hasDate, bool hasArea)
    {
        var command = ValidProfile(isDisplaced: isDisplaced,
            displacementDate: hasDate ? new DateOnly(2020, 1, 1) : null,
            oldAreaId: hasArea ? Guid.NewGuid() : null);

        var result = new SaveBeneficiaryProfileCommandValidator().Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Displaced_WithDateAndArea_IsValid()
    {
        var command = ValidProfile(isDisplaced: true,
            displacementDate: new DateOnly(2020, 1, 1), oldAreaId: Guid.NewGuid());

        Assert.True(new SaveBeneficiaryProfileCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Disabled_RequiresAtLeastOneDisability()
    {
        var command = ValidProfile(isDisabled: true,
            disabilities: Array.Empty<SaveBeneficiaryDisabilityItem>());

        Assert.False(new SaveBeneficiaryProfileCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Disabilities_AreRejectedWhenNotDisabled()
    {
        var command = ValidProfile(isDisabled: false,
            disabilities: new[] { new SaveBeneficiaryDisabilityItem(Guid.NewGuid(), Guid.NewGuid()) });

        Assert.False(new SaveBeneficiaryProfileCommandValidator().Validate(command).IsValid);
    }

    [Theory]
    [InlineData((short)5, null)]
    [InlineData(null, (short)31)]
    public void WivesAndChildren_RangesAreEnforced(short? wives, short? children)
    {
        var command = ValidProfile(wives: wives, children: children);

        Assert.False(new SaveBeneficiaryProfileCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Phone2_MustDifferAfterCanonicalConversion()
    {
        var command = ValidProfile(phone1: "0912345678", phone2: "+963912345678");

        Assert.False(new SaveBeneficiaryProfileCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Phone2_RequiresPhone1()
    {
        var command = ValidProfile(phone2: "0912345678");

        Assert.False(new SaveBeneficiaryProfileCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void MarriedConditionalRules_UseTheSeededCode()
    {
        Assert.True(BeneficiaryFormRules.AllowsWivesNumber(MaritalStatusCodes.Married, true));
        Assert.False(BeneficiaryFormRules.AllowsWivesNumber(MaritalStatusCodes.Married, false));
        Assert.False(BeneficiaryFormRules.AllowsWivesNumber(MaritalStatusCodes.Single, true));
        Assert.True(BeneficiaryFormRules.AllowsChildrenNumber(MaritalStatusCodes.Married));
        Assert.False(BeneficiaryFormRules.AllowsChildrenNumber(MaritalStatusCodes.Single));
    }

    [Fact]
    public void Register_RequiresExplicitGender()
    {
        var command = new RegisterBeneficiaryCommand("Ali", "Hassan", "Karam", "Nour",
            new DateOnly(1990, 5, 4), false, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        var result = new RegisterBeneficiaryCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterBeneficiaryCommand.Gender));
    }

    private static SaveBeneficiaryProfileCommand ValidProfile(
        string? phone1 = null, string? phone2 = null,
        bool isDisplaced = false, DateOnly? displacementDate = null, Guid? oldAreaId = null,
        bool isDisabled = false, IReadOnlyList<SaveBeneficiaryDisabilityItem>? disabilities = null,
        short? wives = null, short? children = null) =>
        new(Guid.NewGuid(), phone1, phone2, isDisplaced, displacementDate, oldAreaId, null,
            isDisabled, disabilities ?? Array.Empty<SaveBeneficiaryDisabilityItem>(), null, null,
            null, null, null, null, null, wives, children, null);
}
