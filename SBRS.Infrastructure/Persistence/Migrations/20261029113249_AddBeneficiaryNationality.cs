using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    // NOTE: written as idempotent raw SQL on purpose.
    // The live database was created from InitialPostgres without EF history bookkeeping
    // (no __EFMigrationsHistory table), so migrations are applied as scripts via psql.
    // The beneficiary table is empty, so the new NOT NULL column needs no backfill.
    public partial class AddBeneficiaryNationality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) column (NOT NULL; enforced below after the add so this stays re-runnable)
            migrationBuilder.Sql("""
                ALTER TABLE beneficiaries ADD COLUMN IF NOT EXISTS nationality_id uuid;
                ALTER TABLE beneficiaries ALTER COLUMN nationality_id SET NOT NULL;
                """);

            // 2) index for nationality filters and reports
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS ix_beneficiaries_nationality_id
                    ON beneficiaries (nationality_id);
                """);

            // 3) FK to nationalities, restrict delete (same rule as the other beneficiary FKs);
            //    name follows the EF snake_case convention so future model diffs stay consistent
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE table_name = 'beneficiaries'
                          AND constraint_name = 'fk_beneficiaries_nationalities_nationality_id'
                          AND constraint_type = 'FOREIGN KEY'
                    ) THEN
                        ALTER TABLE beneficiaries
                            ADD CONSTRAINT fk_beneficiaries_nationalities_nationality_id
                            FOREIGN KEY (nationality_id) REFERENCES nationalities (id)
                            ON DELETE RESTRICT ON UPDATE NO ACTION;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE beneficiaries DROP CONSTRAINT IF EXISTS fk_beneficiaries_nationalities_nationality_id;
                DROP INDEX IF EXISTS ix_beneficiaries_nationality_id;
                ALTER TABLE beneficiaries DROP COLUMN IF EXISTS nationality_id;
                """);
        }
    }
}
