using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // NOTE: written as idempotent raw SQL on purpose.
    // The live database was created from InitialPostgres without EF history bookkeeping
    // (no __EFMigrationsHistory table), so migrations are applied as scripts via psql.
    // The beneficiary table is currently empty; the code column also migrates from the
    // old (accidental) citext type to the integer type the entity property always had.
    public partial class AddPerFacilityBeneficiaryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) beneficiaries.code must be an integer (the entity property is int;
            //    it was accidentally created as citext). tableName has 0 rows so the cast is safe.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'beneficiaries' AND column_name = 'code'
                          AND (data_type = 'USER-DEFINED' OR data_type <> 'integer')
                    ) THEN
                        ALTER TABLE beneficiaries ALTER COLUMN code TYPE integer USING code::text::integer;
                    END IF;
                END $$;
                """);

            // 2) the old system-wide unique index on code is replaced by the per-facility one
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ix_beneficiaries_code;
                CREATE UNIQUE INDEX IF NOT EXISTS ix_beneficiaries_registration_facility_id_code
                    ON beneficiaries (registration_facility_id, code);
                """);

            // 3) ix_beneficiaries_full_code (unique on full_code) already exists from InitialPostgres;
            //    nothing to do for it.

            // 4) the old global generation sequence is obsolete
            migrationBuilder.Sql("""
                DROP SEQUENCE IF EXISTS beneficiary_code_seq;
                """);
        }

        /// <inheritdoc />
        // Best-effort reverse: only valid while the table holds data that can be cast back.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ix_beneficiaries_registration_facility_id_code;
                CREATE UNIQUE INDEX IF NOT EXISTS ix_beneficiaries_code ON beneficiaries (code);

                CREATE SEQUENCE IF NOT EXISTS beneficiary_code_seq;

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'beneficiaries' AND column_name = 'code'
                          AND data_type <> 'USER-DEFINED'
                    ) THEN
                        ALTER TABLE beneficiaries ALTER COLUMN code TYPE citext USING code::text::citext;
                    END IF;
                END $$;
                """);
        }
    }
}
