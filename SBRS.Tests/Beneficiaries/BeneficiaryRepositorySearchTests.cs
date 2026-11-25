using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Infrastructure.Persistence;
using SBRS.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SBRS.Tests.Beneficiaries;

public class BeneficiaryRepositorySearchTests
{
    [Fact]
    public async Task FullCodeOnly_ReturnsTheExactRecord()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", fullCode: "ALP-PHC-001-1");
        var other = TestBeneficiaries.Create(firstName: "علي", fullCode: "ALP-PHC-001-2");
        await db.SeedAsync(wanted, other);

        var result = await SearchAsync(db, Criteria(fullCode: "ALP-PHC-001-1"));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task FirstNameOnly_FiltersByStartsWithOnTheNormalizedColumn()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي");
        var other = TestBeneficiaries.Create(firstName: "علي", lastName: "الحسن");
        await db.SeedAsync(wanted, other);

        var result = await SearchAsync(db, Criteria(firstName: "مح"));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task FatherNameOnly_FiltersByStartsWithOnTheNormalizedColumn()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", fatherName: "أحمد");
        var other = TestBeneficiaries.Create(firstName: "علي", fatherName: "سامر");
        await db.SeedAsync(wanted, other);

        var result = await SearchAsync(db, Criteria(fatherName: "أح"));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task LastNameOnly_FiltersByStartsWithOnTheNormalizedColumn()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي");
        var other = TestBeneficiaries.Create(firstName: "علي", lastName: "الحسن");
        await db.SeedAsync(wanted, other);

        var result = await SearchAsync(db, Criteria(lastName: "الع"));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task MotherNameOnly_FiltersByStartsWithOnTheNormalizedColumn()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", motherName: "فاطمة");
        var other = TestBeneficiaries.Create(firstName: "علي", motherName: "سارة");
        await db.SeedAsync(wanted, other);

        var result = await SearchAsync(db, Criteria(motherName: "فاط"));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task BirthDateOnly_MatchesTheExactDate()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", birthDate: new DateOnly(1990, 5, 4));
        var other = TestBeneficiaries.Create(firstName: "علي", birthDate: new DateOnly(1991, 5, 4));
        await db.SeedAsync(wanted, other);

        var result = await SearchAsync(db, Criteria(birthDate: new DateOnly(1990, 5, 4)));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task AllKeys_AreCombinedWithAnd()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(
            "محمد", "أحمد", "العلي", "فاطمة", new DateOnly(1990, 5, 4));
        var otherMother = TestBeneficiaries.Create(
            "محمد", "أحمد", "العلي", "سارة", new DateOnly(1990, 5, 4));
        var otherFather = TestBeneficiaries.Create(
            "محمد", "سامر", "العلي", "فاطمة", new DateOnly(1990, 5, 4));
        var otherBirthDate = TestBeneficiaries.Create(
            "محمد", "أحمد", "العلي", "فاطمة", new DateOnly(1990, 5, 5));
        await db.SeedAsync(wanted, otherMother, otherFather, otherBirthDate);

        var result = await SearchAsync(db, Criteria(
            firstName: "محمد", fatherName: "أحمد", lastName: "العلي", motherName: "فاطمة",
            birthDate: new DateOnly(1990, 5, 4)));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task MixOfTwoKeys_ReturnsTheirIntersection()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي");
        var otherFirstName = TestBeneficiaries.Create(firstName: "محمد", lastName: "الحسن");
        var otherLastName = TestBeneficiaries.Create(firstName: "علي", lastName: "العلي");
        await db.SeedAsync(wanted, otherFirstName, otherLastName);

        var result = await SearchAsync(db, Criteria(firstName: "محمد", lastName: "العلي"));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task ArabicNormalizationVariants_MatchStoredNormalizedColumns()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(
            firstName: "أَحْمَـد", fatherName: "إبراهيم", lastName: "مصطفى", motherName: "فاطمة");
        await db.SeedAsync(wanted);

        // raw variants: alef without hamza, alef maqsura as ya, ta marbuta as ha, no diacritics
        var result = await SearchAsync(db, Criteria(
            firstName: "احمد", fatherName: "ابراهيم", lastName: "مصطفي", motherName: "فاطمه"));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task BirthDate_IsExact_NoYearOnlyMatching()
    {
        await using var db = await TestDb.CreateAsync();
        var wanted = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي", birthDate: new DateOnly(1990, 5, 4));
        var sameYear = TestBeneficiaries.Create(firstName: "علي", lastName: "العلي", birthDate: new DateOnly(1990, 6, 4));
        await db.SeedAsync(wanted, sameYear);

        var result = await SearchAsync(db, Criteria(lastName: "العلي", birthDate: new DateOnly(1990, 5, 4)));

        Assert.Equal(wanted.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task MergedDuplicates_AreHiddenFromNameSearch()
    {
        await using var db = await TestDb.CreateAsync();
        var survivor = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي", birthDate: new DateOnly(1990, 5, 4));
        var merged = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي", birthDate: new DateOnly(1990, 5, 4),
            status: BeneficiaryMatchReviewStatus.PendingReview, disambiguator: 1);
        merged.MergeInto(survivor.Id, Guid.NewGuid());
        await db.SeedAsync(survivor, merged);

        var result = await SearchAsync(db, Criteria(firstName: "محمد"));

        Assert.Equal(survivor.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task MergedFullCode_ResolvesToTheSurvivor()
    {
        await using var db = await TestDb.CreateAsync();
        var survivor = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي", birthDate: new DateOnly(1990, 5, 4));
        var merged = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي", birthDate: new DateOnly(1990, 5, 4),
            fullCode: "ZZ-OLD-9", status: BeneficiaryMatchReviewStatus.PendingReview, disambiguator: 1);
        merged.MergeInto(survivor.Id, Guid.NewGuid());
        await db.SeedAsync(survivor, merged);

        var result = await SearchAsync(db, Criteria(fullCode: "ZZ-OLD-9"));

        Assert.Equal(survivor.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task MergedFullCode_IsStillCombinedWithAnd()
    {
        await using var db = await TestDb.CreateAsync();
        var survivor = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي", birthDate: new DateOnly(1990, 5, 4));
        var merged = TestBeneficiaries.Create(firstName: "محمد", lastName: "العلي", birthDate: new DateOnly(1990, 5, 4),
            fullCode: "ZZ-OLD-9", status: BeneficiaryMatchReviewStatus.PendingReview, disambiguator: 1);
        merged.MergeInto(survivor.Id, Guid.NewGuid());
        await db.SeedAsync(survivor, merged);

        var result = await SearchAsync(db, Criteria(fullCode: "ZZ-OLD-9", firstName: "عمر"));

        Assert.Empty(result);
    }

    [Fact]
    public async Task Results_AreOrderedByLastNameThenFirstNameThenBirthDate()
    {
        await using var db = await TestDb.CreateAsync();
        var second = TestBeneficiaries.Create(firstName: "ب", fatherName: "مشترك", lastName: "أ", birthDate: new DateOnly(1991, 1, 1));
        var third = TestBeneficiaries.Create(firstName: "ز", fatherName: "مشترك", lastName: "أ", birthDate: new DateOnly(1990, 1, 1));
        var first = TestBeneficiaries.Create(firstName: "ب", fatherName: "مشترك", lastName: "ب", birthDate: new DateOnly(1990, 1, 1));
        await db.SeedAsync(second, third, first);

        var result = await SearchAsync(db, Criteria(fatherName: "مشترك"));

        Assert.Equal(new[] { second.Id, third.Id, first.Id }, result.Select(x => x.Id).ToArray());
    }

    private static BeneficiarySearchCriteria Criteria(
        string? fullCode = null, string? firstName = null, string? fatherName = null,
        string? lastName = null, string? motherName = null, DateOnly? birthDate = null) =>
        new(
            fullCode,
            firstName is null ? null : ArabicTextNormalizer.Normalize(firstName),
            fatherName is null ? null : ArabicTextNormalizer.Normalize(fatherName),
            lastName is null ? null : ArabicTextNormalizer.Normalize(lastName),
            motherName is null ? null : ArabicTextNormalizer.Normalize(motherName),
            birthDate);

    private static async Task<IReadOnlyList<BeneficiaryEntity>> SearchAsync(
        TestDb db, BeneficiarySearchCriteria criteria)
    {
        var repository = new BeneficiaryRepository(db.Context);
        return await repository.SearchAsync(criteria, 21, CancellationToken.None);
    }

    private sealed class TestDb : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDb(SqliteConnection connection, AppDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public AppDbContext Context { get; }

        public static async Task<TestDb> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
            await connection.OpenAsync();

            // The real context gets the snake_case convention from Infrastructure DI; without it
            // the check-constraint SQL (snake_case) does not match the generated column names.
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .UseSnakeCaseNamingConvention()
                .Options;

            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return new TestDb(connection, context);
        }

        public async Task SeedAsync(params BeneficiaryEntity[] items)
        {
            Context.Beneficiaries.AddRange(items);
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
