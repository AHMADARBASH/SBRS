using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillUserBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Facility-level users (FacilityAdmin, DataEntry) are scoped by their facility, and
            // their branch is now derived from that facility. Rows created before this fix were
            // stored with branch_id = NULL, so the user list and the JWT had no branch.
            // Only NULL rows are touched, so the statement is idempotent.
            migrationBuilder.Sql("""
                UPDATE "AspNetUsers" AS u
                SET branch_id = f.branch_id
                FROM facilities AS f
                WHERE u.facility_id = f.id
                  AND u.branch_id IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data repair: deliberately not reverted, the branch is correct for these users.
        }
    }
}
