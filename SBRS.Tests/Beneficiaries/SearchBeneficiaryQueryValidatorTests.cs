using FluentValidation.Results;
using SBRS.Application.Features.Beneficiaries.Queries;
using SBRS.Domain.Common;
using Xunit;

namespace SBRS.Tests.Beneficiaries;

public class SearchBeneficiaryQueryValidatorTests
{
    private const string NoKeyMessage = "At least one search key is required.";

    private static ValidationResult Validate(SearchBeneficiaryQuery query) =>
        new SearchBeneficiaryQueryValidator().Validate(query);

    [Fact]
    public void NoKeys_AreRejected_WithClearMessage()
    {
        var result = Validate(new SearchBeneficiaryQuery(null, null, null, null, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == NoKeyMessage);
    }

    [Fact]
    public void WhitespaceOnlyKeys_AreRejected()
    {
        var result = Validate(new SearchBeneficiaryQuery("   ", "\t", " ", "\r\n", "  ", null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == NoKeyMessage);
    }

    [Fact]
    public void NameThatNormalizesToNothing_CountsAsNotProvided()
    {
        // diacritics + tatweel only: normalization returns an empty string
        var result = Validate(new SearchBeneficiaryQuery(null, "\u064B\u064C\u0640", null, null, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == NoKeyMessage);
    }

    [Fact]
    public void FullCodeAlone_IsValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery("ALP-PHC-001-7", null, null, null, null, null)).IsValid);
    }

    [Fact]
    public void FirstNameAlone_IsValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery(null, "محمد", null, null, null, null)).IsValid);
    }

    [Fact]
    public void FatherNameAlone_IsValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery(null, null, "أحمد", null, null, null)).IsValid);
    }

    [Fact]
    public void LastNameAlone_IsValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery(null, null, null, "العلي", null, null)).IsValid);
    }

    [Fact]
    public void MotherNameAlone_IsValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery(null, null, null, null, "فاطمة", null)).IsValid);
    }

    [Fact]
    public void BirthDateAlone_IsValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery(
            null, null, null, null, null, new DateOnly(1990, 5, 4))).IsValid);
    }

    [Fact]
    public void AllKeysTogether_AreValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery(
            "ALP-PHC-001-7", "محمد", "أحمد", "العلي", "فاطمة", new DateOnly(1990, 5, 4))).IsValid);
    }

    [Fact]
    public void MixOfTwoKeys_IsValid()
    {
        Assert.True(Validate(new SearchBeneficiaryQuery(
            null, "محمد", null, "العلي", null, null)).IsValid);
        Assert.True(Validate(new SearchBeneficiaryQuery(
            null, null, "أحمد", null, "فاطمة", null)).IsValid);
    }

    [Theory]
    [InlineData("أحمد", "احمد")]        // alef with hamza
    [InlineData("إبراهيم", "ابراهيم")]  // alef with hamza below
    [InlineData("آمنة", "امنه")]        // alef with madda + ta marbuta
    [InlineData("مصطفى", "مصطفي")]      // alef maqsura -> ya
    [InlineData("فاطمة", "فاطمه")]      // ta marbuta -> ha
    [InlineData("أَحْمَـد", "احمد")]    // diacritics + tatweel
    public void ArabicNormalizationVariants_AreAccepted(string input, string expectedNormalized)
    {
        var result = Validate(new SearchBeneficiaryQuery(null, input, null, null, null, null));

        Assert.True(result.IsValid);
        Assert.Equal(expectedNormalized, ArabicTextNormalizer.Normalize(input));
    }

    [Theory]
    [InlineData("م", "First name must be at least 2 characters after normalization.")]
    [InlineData("أ", "First name must be at least 2 characters after normalization.")]
    public void ProvidedFirstNameShorterThanMinimum_IsRejected(string value, string message)
    {
        var result = Validate(new SearchBeneficiaryQuery(null, value, null, null, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == message);
    }

    [Fact]
    public void ShortProvidedName_IsRejected_EvenWhenAnotherKeyIsValid()
    {
        var result = Validate(new SearchBeneficiaryQuery(
            "ALP-PHC-001-7", null, "ا", null, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage == "Father name must be at least 2 characters after normalization.");
    }

    [Fact]
    public void TooLongFullCode_IsRejected()
    {
        var result = Validate(new SearchBeneficiaryQuery(new string('A', 51), null, null, null, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SearchBeneficiaryQuery.FullCode));
    }
}
