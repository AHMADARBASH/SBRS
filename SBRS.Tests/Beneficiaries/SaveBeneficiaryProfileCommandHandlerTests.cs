using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.BeneficiaryProfiles.Commands;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;
using Xunit;

namespace SBRS.Tests.Beneficiaries;

public class SaveBeneficiaryProfileCommandHandlerTests
{
    private sealed class FakeProfileRepository : IBeneficiaryProfileRepository
    {
        public BeneficiaryProfileEntity? Profile { get; private set; }

        public Task<BeneficiaryProfileEntity?> GetByBeneficiaryIdAsync(Guid beneficiaryId, CancellationToken ct) =>
            Task.FromResult(Profile);

        public Task<IReadOnlyList<BeneficiaryDisabilityEntity>> GetDisabilitiesAsync(Guid beneficiaryId,
            CancellationToken ct) => Task.FromResult<IReadOnlyList<BeneficiaryDisabilityEntity>>([]);

        public Task<IReadOnlyList<BeneficiaryChronicDiseaseEntity>> GetChronicDiseasesAsync(Guid beneficiaryId,
            CancellationToken ct) => Task.FromResult<IReadOnlyList<BeneficiaryChronicDiseaseEntity>>([]);

        public Task AddAsync(BeneficiaryProfileEntity profile, CancellationToken ct)
        {
            Profile = profile;
            return Task.CompletedTask;
        }

        public Task AddAsync(BeneficiaryDisabilityEntity disability, CancellationToken ct) =>
            Task.CompletedTask;

        public Task AddAsync(BeneficiaryChronicDiseaseEntity chronicDisease, CancellationToken ct) =>
            Task.CompletedTask;

        public void RemoveDisabilities(IEnumerable<BeneficiaryDisabilityEntity> disabilities) { }

        public void RemoveChronicDiseases(IEnumerable<BeneficiaryChronicDiseaseEntity> chronicDiseases) { }
    }

    private sealed class FakeIdentityStore : IBeneficiaryIdentityRepository
    {
        private readonly List<BeneficiaryIdentityEntity> _rows = [];

        public IReadOnlyList<BeneficiaryIdentityEntity> Rows => _rows;

        public void Seed(BeneficiaryIdentityEntity identity) => _rows.Add(identity);

        public Task<IReadOnlyList<BeneficiaryIdentityEntity>> GetByBeneficiaryIdAsync(Guid beneficiaryId,
            CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<BeneficiaryIdentityEntity>>(
                _rows.Where(x => x.BeneficiaryId == beneficiaryId).ToList());

        public Task<IReadOnlyDictionary<Guid, IReadOnlyList<BeneficiaryIdentityEntity>>> GetByBeneficiaryIdsAsync(
            IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken ct) =>
            Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<BeneficiaryIdentityEntity>>>(
                _rows.Where(x => beneficiaryIds.Contains(x.BeneficiaryId))
                    .GroupBy(x => x.BeneficiaryId)
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<BeneficiaryIdentityEntity>)g.ToList()));

        public Task<IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity>> GetPrimaryByBeneficiaryIdsAsync(
            IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken ct) =>
            Task.FromResult<IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity>>(
                _rows.Where(x => beneficiaryIds.Contains(x.BeneficiaryId))
                    .GroupBy(x => x.BeneficiaryId)
                    .ToDictionary(g => g.Key, g => g.First()));

        public Task AddAsync(BeneficiaryIdentityEntity identity, CancellationToken ct)
        {
            _rows.Add(identity);
            return Task.CompletedTask;
        }

        public Task<BeneficiaryIdentityEntity?> GetPrimaryForUpdateAsync(Guid beneficiaryId,
            CancellationToken ct) =>
            Task.FromResult(_rows.FirstOrDefault(x => x.BeneficiaryId == beneficiaryId));
    }

    private sealed class FakeIdentityTypeRepository(params IdentityTypeEntity[] types) : IIdentityTypeRepository
    {
        public Task<IdentityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(types.FirstOrDefault(t => t.Id == id));

        public Task<IReadOnlyList<IdentityTypeEntity>> GetByIdsAsync(IReadOnlyCollection<Guid> ids,
            CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<IdentityTypeEntity>>(
                types.Where(t => ids.Contains(t.Id)).ToList());

        public Task<IReadOnlyList<IdentityTypeEntity>> GetAllAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<IdentityTypeEntity>>(types);

        public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) =>
            Task.FromResult(false);

        public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct) =>
            Task.FromResult(false);

        public Task AddAsync(IdentityTypeEntity identityType, CancellationToken ct) =>
            Task.CompletedTask;
    }

    private sealed class FakeStatusRepository(BeneficiaryStatusEntity status) : IBeneficiaryStatusRepository
    {
        public Task<BeneficiaryStatusEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<BeneficiaryStatusEntity?>(id == status.Id ? status : null);

        public Task<BeneficiaryStatusEntity?> GetByCodeAsync(string code, CancellationToken ct) =>
            Task.FromResult<BeneficiaryStatusEntity?>(code == status.Code ? status : null);

        public Task<IReadOnlyList<BeneficiaryStatusEntity>> GetAllAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<BeneficiaryStatusEntity>>([status]);
    }

    private sealed class FakeMaritalStatusRepository : IMaritalStatusRepository
    {
        public Task<MaritalStatusEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<MaritalStatusEntity?>(null);

        public Task<MaritalStatusEntity?> GetByCodeAsync(string code, CancellationToken ct) =>
            Task.FromResult<MaritalStatusEntity?>(null);

        public Task<IReadOnlyList<MaritalStatusEntity>> GetAllAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<MaritalStatusEntity>>([]);
    }

    private sealed class FakeGeographyRepository : IGeographyRepository
    {
        public Task<List<CountryEntity>> GetCountriesAsync(CancellationToken ct = default) => Task.FromResult<List<CountryEntity>>([]);
        public Task<List<CityEntity>> GetCitiesByCountryAsync(Guid countryId, CancellationToken ct = default) => Task.FromResult<List<CityEntity>>([]);
        public Task<List<RegionEntity>> GetRegionsByCityAsync(Guid cityId, CancellationToken ct = default) => Task.FromResult<List<RegionEntity>>([]);
        public Task<List<DistrictEntity>> GetDistrictsByRegionAsync(Guid regionId, CancellationToken ct = default) => Task.FromResult<List<DistrictEntity>>([]);
        public Task<List<AreaEntity>> GetAreasByDistrictAsync(Guid districtId, CancellationToken ct = default) => Task.FromResult<List<AreaEntity>>([]);
        public Task<List<AreaEntity>> GetAreasByCityAsync(Guid cityId, CancellationToken ct = default) => Task.FromResult<List<AreaEntity>>([]);
        public Task<CityEntity?> GetCityByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<CityEntity?>(null);
        public Task<AreaEntity?> GetAreaByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<AreaEntity?>(null);
        public Task<IReadOnlyDictionary<Guid, AreaChainRow>> GetAreaChainsAsync(
            IReadOnlyCollection<Guid> areaIds, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyDictionary<Guid, AreaChainRow>>(new Dictionary<Guid, AreaChainRow>());
    }

    private sealed class FakeAccommodationTypeRepository : IAccommodationTypeRepository
    {
        public Task<AccommodationTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<AccommodationTypeEntity?>(null);
        public Task<IReadOnlyList<AccommodationTypeEntity>> GetAllAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<AccommodationTypeEntity>>([]);
        public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) => Task.FromResult(false);
        public Task AddAsync(AccommodationTypeEntity accommodationType, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FakeEducationTypeRepository : IEducationTypeRepository
    {
        public Task<EducationTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<EducationTypeEntity?>(null);
        public Task<IReadOnlyList<EducationTypeEntity>> GetAllAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<EducationTypeEntity>>([]);
        public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) => Task.FromResult(false);
        public Task AddAsync(EducationTypeEntity educationType, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FakeEducationIssueRepository : IEducationIssueRepository
    {
        public Task<EducationIssueEntity?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<EducationIssueEntity?>(null);
        public Task<IReadOnlyList<EducationIssueEntity>> GetAllAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<EducationIssueEntity>>([]);
    }

    private sealed class FakeDisabilityTypeRepository : IDisabilityTypeRepository
    {
        public Task<DisabilityTypeEntity?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<DisabilityTypeEntity?>(null);
        public Task<IReadOnlyList<DisabilityTypeEntity>> GetAllAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<DisabilityTypeEntity>>([]);
        public Task<bool> EnNameExistsAsync(string enName, Guid? excludeId, CancellationToken ct) => Task.FromResult(false);
        public Task AddAsync(DisabilityTypeEntity disabilityType, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FakeDisabilityReasonRepository : IDisabilityReasonRepository
    {
        public Task<DisabilityReasonEntity?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<DisabilityReasonEntity?>(null);
        public Task<IReadOnlyList<DisabilityReasonEntity>> GetAllAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<DisabilityReasonEntity>>([]);
    }

    private sealed class FakeChronicDiseaseRepository : IChronicDiseaseRepository
    {
        public Task<ChronicDiseaseEntity?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult<ChronicDiseaseEntity?>(null);
        public Task<IReadOnlyList<ChronicDiseaseEntity>> GetAllAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<ChronicDiseaseEntity>>([]);
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
        public FakeIdentityStore Identities { get; } = new();
        public FakeProfileRepository Profiles { get; } = new();
        public FakeEnrollmentRepository Enrollments { get; } = new();
        public FakeCurrentUser CurrentUser { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();

        public BeneficiaryStatusEntity Status { get; } =
            new("غير متاح", "Not available", BeneficiaryStatusCodes.NotAvailable);

        public IdentityTypeEntity NationalId { get; } =
            new("هوية وطنية", "National ID", IdentityTypeCodes.NationalId, @"^\d{9}$");

        public IdentityTypeEntity Passport { get; } =
            new("جواز سفر", "Passport", IdentityTypeCodes.Passport, @"^[A-Z0-9]{6,12}$");

        public IdentityTypeEntity Unregistered { get; } =
            new("غير مسجل", "Unregistered", IdentityTypeCodes.Unregistered, @"^$");

        public Harness()
        {
            Status.Id = Guid.NewGuid();
            NationalId.Id = Guid.NewGuid();
            Passport.Id = Guid.NewGuid();
            Unregistered.Id = Guid.NewGuid();
            CurrentUser.FacilityId = Guid.NewGuid();
        }

        public SaveBeneficiaryProfileCommandHandler CreateHandler() =>
            new(Beneficiaries, Identities,
                new FakeIdentityTypeRepository(NationalId, Passport, Unregistered),
                Profiles, Enrollments,
                new FakeGeographyRepository(),
                new FakeAccommodationTypeRepository(),
                new FakeEducationTypeRepository(),
                new FakeEducationIssueRepository(),
                new FakeDisabilityTypeRepository(),
                new FakeDisabilityReasonRepository(),
                new FakeChronicDiseaseRepository(),
                new FakeStatusRepository(Status),
                new FakeMaritalStatusRepository(),
                CurrentUser, UnitOfWork);

        public BeneficiaryEntity Enroll()
        {
            var beneficiary = TestBeneficiaries.Create("Ali", "Hassan", "Karam", "Nour",
                new DateOnly(1990, 5, 4));
            Beneficiaries.ById = beneficiary;
            Enrollments.Enrolled.Add(beneficiary.Id);
            return beneficiary;
        }

        public void SeedIdentity(BeneficiaryEntity beneficiary, IdentityTypeEntity type, string value) =>
            Identities.Seed(new BeneficiaryIdentityEntity(beneficiary.Id, type, value));
    }

    private static SaveBeneficiaryProfileCommand Command(BeneficiaryEntity beneficiary,
        IReadOnlyList<SaveBeneficiaryNewIdentityItem>? newIdentities = null) =>
        new(beneficiary.Id, null, null, false, null, null, null, false,
            Array.Empty<SaveBeneficiaryDisabilityItem>(), null, null, null, null,
            null, null, null, null, null, newIdentities);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NoNewIdentities_ChangesNothing(bool sendEmptyList)
    {
        var harness = new Harness();
        var beneficiary = harness.Enroll();

        await harness.CreateHandler().Handle(
            Command(beneficiary, sendEmptyList ? [] : null), default);

        Assert.Empty(harness.Identities.Rows);
        Assert.Equal(1, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task OneNewType_IsInserted_AndExistingRowsAreKept()
    {
        var harness = new Harness();
        var beneficiary = harness.Enroll();
        harness.SeedIdentity(beneficiary, harness.NationalId, "123456789");

        await harness.CreateHandler().Handle(Command(beneficiary,
        [
            new SaveBeneficiaryNewIdentityItem(harness.Passport.Id, "AB123456")
        ]), default);

        Assert.Equal(2, harness.Identities.Rows.Count);
        var existing = Assert.Single(harness.Identities.Rows, x => x.IdentityTypeId == harness.NationalId.Id);
        Assert.Equal("123456789", existing.Value);
        var added = Assert.Single(harness.Identities.Rows, x => x.IdentityTypeId == harness.Passport.Id);
        Assert.Equal("AB123456", added.Value);
        Assert.Equal(1, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task TwoNewTypes_AreBothInserted()
    {
        var harness = new Harness();
        var beneficiary = harness.Enroll();

        await harness.CreateHandler().Handle(Command(beneficiary,
        [
            new SaveBeneficiaryNewIdentityItem(harness.NationalId.Id, "123456789"),
            new SaveBeneficiaryNewIdentityItem(harness.Passport.Id, "AB123456")
        ]), default);

        Assert.Equal(2, harness.Identities.Rows.Count);
        Assert.Contains(harness.Identities.Rows, x => x.IdentityTypeId == harness.NationalId.Id);
        Assert.Contains(harness.Identities.Rows, x => x.IdentityTypeId == harness.Passport.Id);
        Assert.Equal(1, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task AlreadyRegisteredType_ReturnsConflict_AndSavesNothing()
    {
        var harness = new Harness();
        var beneficiary = harness.Enroll();
        harness.SeedIdentity(beneficiary, harness.NationalId, "123456789");

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.CreateHandler().Handle(Command(beneficiary,
            [
                new SaveBeneficiaryNewIdentityItem(harness.NationalId.Id, "987654321"),
                new SaveBeneficiaryNewIdentityItem(harness.Passport.Id, "AB123456")
            ]), default));

        Assert.Contains(IdentityTypeCodes.NationalId, error.Message);
        Assert.Single(harness.Identities.Rows);
        Assert.Equal(0, harness.UnitOfWork.Saves);
    }

    [Fact]
    public void DuplicateTypeInsideTheList_IsRejectedByTheValidator()
    {
        var harness = new Harness();
        var beneficiary = TestBeneficiaries.Create();

        var result = new SaveBeneficiaryProfileCommandValidator().Validate(Command(beneficiary,
        [
            new SaveBeneficiaryNewIdentityItem(harness.NationalId.Id, "123456789"),
            new SaveBeneficiaryNewIdentityItem(harness.NationalId.Id, "987654321")
        ]));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SaveBeneficiaryProfileCommand.NewIdentities));
    }

    [Fact]
    public async Task Unregistered_WhenBeneficiaryHasAnIdentity_IsRejected()
    {
        var harness = new Harness();
        var beneficiary = harness.Enroll();
        harness.SeedIdentity(beneficiary, harness.NationalId, "123456789");

        await Assert.ThrowsAsync<DomainException>(() =>
            harness.CreateHandler().Handle(Command(beneficiary,
            [
                new SaveBeneficiaryNewIdentityItem(harness.Unregistered.Id, null)
            ]), default));

        Assert.Single(harness.Identities.Rows);
        Assert.Equal(0, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task InvalidValue_ForItsType_IsRejected()
    {
        var harness = new Harness();
        var beneficiary = harness.Enroll();

        await Assert.ThrowsAsync<DomainException>(() =>
            harness.CreateHandler().Handle(Command(beneficiary,
            [
                new SaveBeneficiaryNewIdentityItem(harness.NationalId.Id, "12AB")
            ]), default));

        Assert.Empty(harness.Identities.Rows);
        Assert.Equal(0, harness.UnitOfWork.Saves);
    }

    [Fact]
    public async Task OutOfScopeBeneficiary_ReturnsNotFound()
    {
        var harness = new Harness();
        var beneficiary = TestBeneficiaries.Create();
        harness.Beneficiaries.ById = beneficiary;   // not enrolled at the user's facility

        await Assert.ThrowsAsync<NotFoundException>(() =>
            harness.CreateHandler().Handle(Command(beneficiary,
            [
                new SaveBeneficiaryNewIdentityItem(harness.NationalId.Id, "123456789")
            ]), default));

        Assert.Empty(harness.Identities.Rows);
        Assert.Equal(0, harness.UnitOfWork.Saves);
    }
}
