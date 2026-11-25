using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SBRS.Application;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.Queries;
using SBRS.Domain.Common;
using SBRS.Domain.Interfaces;
using Xunit;

namespace SBRS.Tests.Beneficiaries;

public class SearchBeneficiaryQueryHandlerTests
{
    private static SearchBeneficiaryQueryHandler CreateHandler(
        FakeBeneficiaryRepository repository,
        FakeIdentityRepository identities,
        FakeEnrollmentRepository enrollments,
        FakeCurrentUser currentUser) =>
        new(repository, identities, enrollments, currentUser);

    [Fact]
    public async Task AllKeys_AreNormalizedAndPassedToTheRepository()
    {
        var repository = new FakeBeneficiaryRepository();
        var identities = new FakeIdentityRepository();
        var currentUser = new FakeCurrentUser();
        var beneficiary = TestBeneficiaries.Create();

        repository.SearchResults = new[] { beneficiary };
        identities.Add(TestBeneficiaries.IdentitiesFor(new[] { beneficiary })[beneficiary.Id]);

        var birthDate = new DateOnly(1990, 5, 4);
        var handler = CreateHandler(repository, identities, new FakeEnrollmentRepository(), currentUser);

        var result = await handler.Handle(new SearchBeneficiaryQuery(
            "alp-phc-001-7", "أَحْمَد", "إبراهيم", "مصطفى", "فاطمة", birthDate), default);

        var criteria = Assert.IsType<BeneficiarySearchCriteria>(repository.LastCriteria);
        Assert.Equal("ALP-PHC-001-7", criteria.FullCode); // case-insensitive input, upper-case criteria
        Assert.Equal("احمد", criteria.NormalizedFirstName);
        Assert.Equal("ابراهيم", criteria.NormalizedFatherName);
        Assert.Equal("مصطفي", criteria.NormalizedLastName);
        Assert.Equal("فاطمه", criteria.NormalizedMotherName);
        Assert.Equal(birthDate, criteria.BirthDate);
        Assert.Equal(21, repository.LastTake); // limit + 1 to detect HasMore
        Assert.Single(result.Items);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task OnlyProvidedKeys_ArePassedToTheRepository()
    {
        var repository = new FakeBeneficiaryRepository();
        var identities = new FakeIdentityRepository();
        var beneficiary = TestBeneficiaries.Create();
        repository.SearchResults = new[] { beneficiary };
        identities.Add(TestBeneficiaries.IdentitiesFor(new[] { beneficiary })[beneficiary.Id]);

        var handler = CreateHandler(repository, identities, new FakeEnrollmentRepository(), new FakeCurrentUser());

        await handler.Handle(new SearchBeneficiaryQuery(null, "محمد", null, "العلي", null, null), default);

        var criteria = repository.LastCriteria!;
        Assert.Null(criteria.FullCode);
        Assert.Equal("محمد", criteria.NormalizedFirstName);
        Assert.Null(criteria.NormalizedFatherName);
        Assert.Equal("العلي", criteria.NormalizedLastName);
        Assert.Null(criteria.NormalizedMotherName);
        Assert.Null(criteria.BirthDate);
    }

    [Fact]
    public async Task MoreThanTwentyMatches_SetsHasMore_AndReturnsTwenty()
    {
        var repository = new FakeBeneficiaryRepository();
        var identities = new FakeIdentityRepository();
        var beneficiaries = Enumerable.Range(0, 21)
            .Select(i => TestBeneficiaries.Create(firstName: $"اسم{i}", lastName: "مشترك"))
            .ToList();

        repository.SearchResults = beneficiaries;
        foreach (var identity in TestBeneficiaries.IdentitiesFor(beneficiaries))
            identities.Add(identity.Value);

        var handler = CreateHandler(repository, identities, new FakeEnrollmentRepository(), new FakeCurrentUser());

        var result = await handler.Handle(new SearchBeneficiaryQuery(null, "اسم", null, null, null, null), default);

        Assert.Equal(20, result.Items.Count);
        Assert.True(result.HasMore);
    }

    [Fact]
    public async Task ExactlyTwentyMatches_DoesNotSetHasMore()
    {
        var repository = new FakeBeneficiaryRepository();
        var identities = new FakeIdentityRepository();
        var beneficiaries = Enumerable.Range(0, 20)
            .Select(i => TestBeneficiaries.Create(firstName: $"اسم{i}", lastName: "مشترك"))
            .ToList();

        repository.SearchResults = beneficiaries;
        foreach (var identity in TestBeneficiaries.IdentitiesFor(beneficiaries))
            identities.Add(identity.Value);

        var handler = CreateHandler(repository, identities, new FakeEnrollmentRepository(), new FakeCurrentUser());

        var result = await handler.Handle(new SearchBeneficiaryQuery(null, "اسم", null, null, null, null), default);

        Assert.Equal(20, result.Items.Count);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task EnrollmentFlag_IsSetForTheCallersFacility()
    {
        var repository = new FakeBeneficiaryRepository();
        var identities = new FakeIdentityRepository();
        var enrollments = new FakeEnrollmentRepository();
        var currentUser = new FakeCurrentUser { FacilityId = Guid.NewGuid() };
        var beneficiary = TestBeneficiaries.Create();

        repository.SearchResults = new[] { beneficiary };
        identities.Add(TestBeneficiaries.IdentitiesFor(new[] { beneficiary })[beneficiary.Id]);
        enrollments.Enrolled.Add(beneficiary.Id);

        var handler = CreateHandler(repository, identities, enrollments, currentUser);

        var result = await handler.Handle(new SearchBeneficiaryQuery(null, "محمد", null, null, null, null), default);

        Assert.True(Assert.Single(result.Items).IsEnrolledInMyFacility);
    }

    [Fact]
    public async Task NoKeys_IsRejectedBeforeTheRepositoryIsQueried()
    {
        var repository = new FakeBeneficiaryRepository();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationDI();
        services.AddSingleton<IBeneficiaryRepository>(repository);
        services.AddSingleton<IBeneficiaryIdentityRepository>(new FakeIdentityRepository());
        services.AddSingleton<IFacilityBeneficiaryRepository>(new FakeEnrollmentRepository());
        services.AddSingleton<ICurrentUser>(new FakeCurrentUser());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ValidationException>(() =>
            mediator.Send(new SearchBeneficiaryQuery(null, null, null, null, null, null)));

        Assert.Equal(0, repository.SearchCalls);
    }
}
