using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // NOTE: this migration is written as idempotent raw SQL on purpose.
    // The live database was created from InitialPostgres without EF history bookkeeping
    // (no __EFMigrationsHistory table), so migrations are applied as scripts instead of
    // "dotnet ef database update". The live database already had a nullable cities.c_code
    // populated with the values below, hence the backfill before SET NOT NULL.
    public partial class AddFacilityCodeGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) add the column when missing (already exists as nullable on the live database)
            migrationBuilder.Sql("""
                ALTER TABLE cities ADD COLUMN IF NOT EXISTS c_code character varying(3) NOT NULL DEFAULT '';
                """);

            // 2) backfill the 14 Syrian governorate cities, matched by the cities' existing code column
            migrationBuilder.Sql("""
                UPDATE cities SET c_code = 'DAM' WHERE code = 'SY01';
                UPDATE cities SET c_code = 'ALP' WHERE code = 'SY02';
                UPDATE cities SET c_code = 'RUD' WHERE code = 'SY03';
                UPDATE cities SET c_code = 'HMS' WHERE code = 'SY04';
                UPDATE cities SET c_code = 'HAM' WHERE code = 'SY05';
                UPDATE cities SET c_code = 'LAT' WHERE code = 'SY06';
                UPDATE cities SET c_code = 'IDL' WHERE code = 'SY07';
                UPDATE cities SET c_code = 'HAS' WHERE code = 'SY08';
                UPDATE cities SET c_code = 'DRZ' WHERE code = 'SY09';
                UPDATE cities SET c_code = 'TAR' WHERE code = 'SY10';
                UPDATE cities SET c_code = 'RAQ' WHERE code = 'SY11';
                UPDATE cities SET c_code = 'DAR' WHERE code = 'SY12';
                UPDATE cities SET c_code = 'SWE' WHERE code = 'SY13';
                UPDATE cities SET c_code = 'QUN' WHERE code = 'SY14';

                -- any city without a known c_code gets '' so SET NOT NULL cannot fail
                UPDATE cities SET c_code = '' WHERE c_code IS NULL;
                """);

            // 3) enforce NOT NULL now that the column is backfilled
            migrationBuilder.Sql("""
                ALTER TABLE cities ALTER COLUMN c_code SET NOT NULL;
                """);

            // 4) unique index, same name EF Core generates for HasIndex(x => x.C_Code).IsUnique()
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS ix_cities_c_code ON cities (c_code);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ix_cities_c_code;
                ALTER TABLE cities DROP COLUMN IF EXISTS c_code;
                """);
        }
    }
}
