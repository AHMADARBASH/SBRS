using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beneficiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MotherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsBirthDateEstimated = table.Column<bool>(type: "bit", nullable: false),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    NormalizedFirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedFatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedMotherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatchKey = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    Disambiguator = table.Column<int>(type: "int", nullable: false),
                    DistinctReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<int>(type: "int", nullable: false),
                    FullCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistrationFacilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beneficiaries_Facilities_RegistrationFacilityId",
                        column: x => x.RegistrationFacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_FullCode",
                table: "Beneficiaries",
                column: "FullCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_MatchKey_Disambiguator",
                table: "Beneficiaries",
                columns: new[] { "MatchKey", "Disambiguator" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_NormalizedLastName_BirthDate",
                table: "Beneficiaries",
                columns: new[] { "NormalizedLastName", "BirthDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_RegistrationFacilityId_Code",
                table: "Beneficiaries",
                columns: new[] { "RegistrationFacilityId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beneficiaries");
        }
    }
}
