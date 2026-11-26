using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Interfaces;

namespace SBRS.Tests.Beneficiaries;

internal sealed class FakeBeneficiaryRepository : IBeneficiaryRepository
{
    public int SearchCalls { get; private set; }
    public BeneficiarySearchCriteria? LastCriteria { get; private set; }
    public int LastTake { get; private set; }
    public IReadOnlyList<BeneficiaryEntity> SearchResults { get; set; } = Array.Empty<BeneficiaryEntity>();
    public Func<BeneficiarySearchCriteria, int, IReadOnlyList<BeneficiaryEntity>>? SearchHandler { get; set; }

    public Task<IReadOnlyList<BeneficiaryEntity>> SearchAsync(BeneficiarySearchCriteria criteria, int take,
        CancellationToken cancellationToken)
    {
        SearchCalls++;
        LastCriteria = criteria;
        LastTake = take;
        return Task.FromResult(SearchHandler is null ? SearchResults : SearchHandler(criteria, take));
    }

    public Task<BeneficiaryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(ById is not null && ById.Id == id ? ById : null);

    public BeneficiaryEntity? ById { get; set; }

    public Task<IReadOnlyList<BeneficiaryEntity>> GetByMatchKeyAsync(string matchKey,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<IReadOnlyList<BeneficiaryEntity>> GetProbableCandidatesAsync(BeneficiaryIdentityKey key,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<int> GetNextDisambiguatorAsync(string matchKey, CancellationToken ct) =>
        Task.FromResult(NextDisambiguator);

    public int NextDisambiguator { get; set; }

    public Task<IReadOnlyList<BeneficiaryEntity>> GetByExactIdentityAsync(
        IReadOnlyCollection<(Guid IdentityTypeId, string Value)> identities, Guid? excludeBeneficiaryId,
        CancellationToken ct) => throw new NotSupportedException();

    public Task LockRegistrationFacilityAsync(Guid facilityId, CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<int> GetMaxCodeForFacilityAsync(Guid facilityId, CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<BeneficiaryEntity>> GetPendingReviewAsync(Guid? facilityId,
        CancellationToken ct) => throw new NotSupportedException();

    public Task<BeneficiaryEntity?> GetByFullCodeAsync(string fullCode, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<(IReadOnlyList<BeneficiaryEntity> Items, int TotalCount)> GetPagedByFacilityAsync(
        Guid facilityId, int page, int pageSize, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task AddAsync(BeneficiaryEntity beneficiary, CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}

internal sealed class FakeIdentityRepository : IBeneficiaryIdentityRepository
{
    private readonly Dictionary<Guid, BeneficiaryIdentityEntity> _items = new();

    public void Add(BeneficiaryIdentityEntity identity) => _items[identity.BeneficiaryId] = identity;

    public Task<IReadOnlyList<BeneficiaryIdentityEntity>> GetByBeneficiaryIdAsync(Guid beneficiaryId,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<BeneficiaryIdentityEntity>>(
            _items.TryGetValue(beneficiaryId, out var identity) ? new[] { identity } : Array.Empty<BeneficiaryIdentityEntity>());

    public Task<IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity>> GetPrimaryByBeneficiaryIdsAsync(
        IReadOnlyCollection<Guid> beneficiaryIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity>>(
            beneficiaryIds.Where(_items.ContainsKey).ToDictionary(id => id, id => _items[id]));

    public Task<BeneficiaryIdentityEntity?> GetPrimaryForUpdateAsync(Guid beneficiaryId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_items.TryGetValue(beneficiaryId, out var identity) ? identity : null);

    public Task AddAsync(BeneficiaryIdentityEntity identity, CancellationToken cancellationToken)
    {
        _items[identity.BeneficiaryId] = identity;
        return Task.CompletedTask;
    }
}

internal sealed class FakeEnrollmentRepository : IFacilityBeneficiaryRepository
{
    public HashSet<Guid> Enrolled { get; } = new();

    public Task<bool> IsEnrolledAsync(Guid beneficiaryId, Guid facilityId, CancellationToken ct) =>
        Task.FromResult(Enrolled.Contains(beneficiaryId));

    public Task<FacilityBeneficiaryEntity?> GetAsync(Guid beneficiaryId, Guid facilityId, CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<FacilityBeneficiaryEntity>> GetByBeneficiaryIdAsync(Guid beneficiaryId,
        CancellationToken ct) => throw new NotSupportedException();

    public Task<IReadOnlySet<Guid>> GetEnrolledBeneficiaryIdsAsync(
        IReadOnlyCollection<Guid> beneficiaryIds, Guid facilityId, CancellationToken ct) =>
        Task.FromResult<IReadOnlySet<Guid>>(beneficiaryIds.Where(Enrolled.Contains).ToHashSet());

    public Task AddAsync(FacilityBeneficiaryEntity enrollment, CancellationToken ct) =>
        throw new NotSupportedException();

    public void Remove(FacilityBeneficiaryEntity enrollment) => throw new NotSupportedException();
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public Guid? UserId { get; set; } = Guid.NewGuid();
    public string? Role { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? FacilityId { get; set; }
    public bool IsSystemAdmin { get; set; }
}

internal static class TestBeneficiaries
{
    private static int _code = 100;

    public static BeneficiaryEntity Create(
        string firstName = "محمد",
        string fatherName = "أحمد",
        string lastName = "العلي",
        string motherName = "فاطمة",
        DateOnly? birthDate = null,
        string? fullCode = null,
        BeneficiaryMatchReviewStatus status = BeneficiaryMatchReviewStatus.NoMatchFound,
        int disambiguator = 0)
    {
        var code = Interlocked.Increment(ref _code);
        var entity = new BeneficiaryEntity(
            firstName, fatherName, lastName, motherName,
            birthDate ?? new DateOnly(1990, 5, 4), false, true,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            Guid.NewGuid(), Guid.NewGuid(),
            code, fullCode ?? $"ZZ-TEST-{code}", Guid.NewGuid(), disambiguator, status);

        // BaseEntity.Id has a public setter; repositories normally assign it.
        entity.Id = Guid.NewGuid();
        return entity;
    }

    public static IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity> IdentitiesFor(
        IEnumerable<BeneficiaryEntity> beneficiaries)
    {
        var type = new IdentityTypeEntity("هوية وطنية", "National ID", "NID", @"^\d{9}$");
        type.Id = Guid.NewGuid();

        var result = new Dictionary<Guid, BeneficiaryIdentityEntity>();
        foreach (var beneficiary in beneficiaries)
        {
            var identity = new BeneficiaryIdentityEntity(beneficiary.Id, type, "123456789");
            identity.Id = Guid.NewGuid();
            // IdentityType is loaded with Include() in the repository; set it for the mapping.
            typeof(BeneficiaryIdentityEntity)
                .GetProperty(nameof(BeneficiaryIdentityEntity.IdentityType))!
                .SetValue(identity, type);
            result[beneficiary.Id] = identity;
        }

        return result;
    }
}
