using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OneActiveVisitPerBeneficiaryFacilityDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_visits_facility_id_beneficiary_id_visit_date",
                table: "visits");

            migrationBuilder.CreateIndex(
                name: "ix_visits_facility_id_beneficiary_id_visit_date",
                table: "visits",
                columns: new[] { "facility_id", "beneficiary_id", "visit_date" },
                unique: true,
                filter: "is_active = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_visits_facility_id_beneficiary_id_visit_date",
                table: "visits");

            migrationBuilder.CreateIndex(
                name: "ix_visits_facility_id_beneficiary_id_visit_date",
                table: "visits",
                columns: new[] { "facility_id", "beneficiary_id", "visit_date" });
        }
    }
}
