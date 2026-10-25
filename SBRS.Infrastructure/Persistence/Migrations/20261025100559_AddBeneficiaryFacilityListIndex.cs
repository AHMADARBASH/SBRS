using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaryFacilityListIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_RegistrationFacilityId_CreatedAt",
                table: "Beneficiaries",
                columns: new[] { "RegistrationFacilityId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_RegistrationFacilityId_CreatedAt",
                table: "Beneficiaries");
        }
    }
}
