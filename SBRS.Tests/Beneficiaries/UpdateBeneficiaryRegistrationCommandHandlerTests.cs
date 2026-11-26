using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.Commands;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;
using Xunit;

namespace SBRS.Tests.Beneficiaries;

public class UpdateBeneficiaryRegistrationCommandHandlerTests
{
    private sealed class FakeGeographyRepository(AreaEntity area) : IGeographyRepository
    {
        public Task<AreaEntity?> GetAreaByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<AreaEntity?>(id == area.Id ? area : null);

        public Task<List<CountryEntity>> GetCountriesAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<List<CityEntity>> GetCitiesByCountryAsync(Guid countryId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<List<RegionEntity>> GetRegionsByCityAsync(Guid cityId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<List<DistrictEntity>> GetDistrictsByRegionAsync(Guid regionId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<List<AreaEntity>> GetAreasByDistrictAsync(Guid districtId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<List<AreaEntity>> GetAreasByCityAsync(Guid cityId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<CityEntity?> GetCityByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class FakeNationalityRepository(NationalityEntity nationality) : INationalityRepository
    {
        public Task<NationalityEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<NationalityEntity?>(id == nationality.Id ? nationality : null);

        public Task<IReadOnlyList<NationalityEntity>> GetAllAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) => throw new NotSupportedException();
        public Task AddAsync(NationalityEntity entity, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeIdentityTypeRepository(IdentityTypeEntity identityType) : IIdentityTypeRepository
    {
        public Task<IdentityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<IdentityTypeEntity?>(id == identityType.Id ? identityType : null);

        public Task<IReadOnlyList<IdentityTypeEntity>> GetByIdsAsync(IReadOnlyCollection<Guid> ids,
            CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<IdentityTypeEntity>>(
                ids.Contains(identityType.Id) ? [identityType] : []);

        public Task<IReadOnlyList<IdentityTypeEntity>> GetAllAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) => throw new NotSupportedException();
        public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) => throw new NotSupportedException();
        public Task AddAsync(IdentityTypeEntity entity, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeMatchService : IBeneficiaryMatchService
    {
        public int Calls { get; private set; }
        public Guid? LastExclude { get; private set; }
        public BeneficiaryIdentityKey? LastKey { get; private set; }
        public BeneficiaryMatchSearchResult Result { get; set; } = BeneficiaryMatchSearchResult.NotFound;

        public Task<BeneficiaryMatchSearchResult> FindAsync(BeneficiaryIdentityKey key, bool isBirthDateEstimated,
            Guid? excludeBeneficiaryId, CancellationToken cancellationToken,
            IReadOnlyCollection<(Guid IdentityTypeId, string Value)>? identities = null)
        {
            Calls++;
            LastExclude = excludeBeneficiaryId;
            LastKey = key;
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int Saves { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            Saves++;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task CommitAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task RollbackAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class Harness
    {
        public FakeBeneficiaryRepository Beneficiaries { get; } = new();
        public FakeIdentityRepository Identities { get; } = new();
        public FakeEnrollmentRepository Enrollments { get; } = new();
        public FakeCurrentUser CurrentUser { get; } = new();
        public FakeMatchService Match { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();
        public AreaEntity Area { get; } = new(Guid.NewGuid(), Guid.NewGuid(), "area", "Area", "AR-1");
        public NationalityEntity Nationality { get; } = new("nationality", "Nationality");
        public IdentityTypeEntity IdentityType { get; } = new("identity", "National ID", "NID", @"^\d{9}$");

        public Harness()
        {
            Area.Id = Guid.NewGuid();
            Nationality.Id = Guid.NewGuid();
            IdentityType.Id = Guid.NewGuid();
            CurrentUser.FacilityId = Guid.NewGuid();
        }

        public UpdateBeneficiaryRegistrationCommandHandler CreateHandler() =>
            new(Beneficiaries, Enrollments,
                new FakeGeographyRepository(Area),
                new FakeIdentityTypeRepository(IdentityType),
                new FakeNationalityRepository(Nationality),
                Identities, Match, CurrentUser, UnitOfWork);

        public void Enroll(BeneficiaryEntity beneficiary)
        {
            Beneficiaries.ById = beneficiary;
            Enrollments.Enrolled.Add(beneficiary.Id);
            Identities.Add(new BeneficiaryIdentityEntity(beneficiary.Id, IdentityType, "123456789"));
        }
    }

    private static UpdateBeneficiaryRegistrationCommand Command(Harness harness, BeneficiaryEntity beneficiary,
        string lastName = "Karam", string identityValue = "123456789") =>
        new(beneficiary.Id, "Ali", "Hassan", lastName, "Nour", new DateOnly(1990, 5, 4), false, true,
            harness.Area.Id, harness.Nationality.Id, harness.IdentityType.Id, identityValue);

    private static BeneficiaryEntity Create(string lastName = "Karam",
        BeneficiaryMatchReviewStatus status = BeneficiaryMatchReviewStatus.NoMatchFound) =>
        TestBeneficiaries.Create("Ali", "Hassan", lastName, "Nour",
            new DateOnly(1990, 5, 4), status: status);

    [Fact]
    public async Task UnchangedKeyFields_DoNotTouchMatchDataOrStatus()
    {
        var harness = new Harness();
        var beneficiary = Create(status: BeneficiaryMatchReviewStatus.PendingReview);
        harness.Enroll(beneficiary);
        beneficiary.ConfirmNotAMatch("checked by facility admin", Guid.NewGuid());
        var matchKeyBefore = beneficiary.MatchKey;
        var statusBefore = beneficiary.MatchReviewStatus;

        // same normalized key, only the area changes
        await harness.CreateHandler().Handle(Command(harness, beneficiary), default);

        Assert.Equal(0, harness.Match.Calls);
        Assert.Equal(statusBefore, beneficiary.MatchReviewStatus);
        Assert.Equal(matchKeyBefore, beneficiary.MatchKey);
        Assert.Equal(1, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task ChangedName_RecomputesMatchKey_AndSetsNoMatchFound()
    {
        var harness = new Harness();
        var beneficiary = Create();
        harness.Enroll(beneficiary);
        var matchKeyBefore = beneficiary.MatchKey;
        harness.Match.Result = BeneficiaryMatchSearchResult.NotFound;

        await harness.CreateHandler().Handle(Command(harness, beneficiary, lastName: "KaramUpdated"), default);

        Assert.Equal(1, harness.Match.Calls);
        Assert.Equal(beneficiary.Id, harness.Match.LastExclude);
        Assert.NotEqual(matchKeyBefore, beneficiary.MatchKey);
        Assert.Equal("karamupdated", beneficiary.NormalizedLastName);
        Assert.Equal(BeneficiaryMatchReviewStatus.NoMatchFound, beneficiary.MatchReviewStatus);
        Assert.Equal(0, beneficiary.Disambiguator);
    }

    [Fact]
    public async Task ChangedName_WithExactMatch_SetsPendingReview_WithNextDisambiguator()
    {
        var harness = new Harness();
        var beneficiary = Create();
        harness.Enroll(beneficiary);
        harness.Match.Result = new BeneficiaryMatchSearchResult(
            BeneficiaryMatchStatus.Exact, Array.Empty<BeneficiaryMatch>());
        harness.Beneficiaries.NextDisambiguator = 3;

        await harness.CreateHandler().Handle(Command(harness, beneficiary, lastName: "KaramUpdated"), default);

        Assert.Equal(BeneficiaryMatchReviewStatus.PendingReview, beneficiary.MatchReviewStatus);
        Assert.Equal(3, beneficiary.Disambiguator);
    }

    [Fact]
    public async Task BeneficiaryOutsideTheFacility_ReturnsNotFound()
    {
        var harness = new Harness();
        var beneficiary = Create();
        harness.Beneficiaries.ById = beneficiary;   // not enrolled at the user's facility

        await Assert.ThrowsAsync<NotFoundException>(() =>
            harness.CreateHandler().Handle(Command(harness, beneficiary), default));

        Assert.Equal(0, harness.Match.Calls);
        Assert.Equal(0, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task MergedBeneficiary_ReturnsNotFound()
    {
        var harness = new Harness();
        var beneficiary = Create(status: BeneficiaryMatchReviewStatus.PendingReview);
        beneficiary.MergeInto(Guid.NewGuid(), Guid.NewGuid());
        harness.Enroll(beneficiary);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            harness.CreateHandler().Handle(Command(harness, beneficiary), default));

        Assert.Equal(0, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task InvalidIdentityValue_IsRejectedAsBusinessRuleViolation()
    {
        var harness = new Harness();
        var beneficiary = Create();
        harness.Enroll(beneficiary);

        // DomainException maps to HTTP 400 in GlobalExceptionHandler.
        await Assert.ThrowsAsync<DomainException>(() =>
            harness.CreateHandler().Handle(Command(harness, beneficiary, identityValue: "12AB"), default));

        Assert.Equal(0, harness.Match.Calls);
        Assert.Equal(0, harness.UnitOfWork.Saves);
    }
}
