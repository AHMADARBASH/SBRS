using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaryRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_RegistrationFacilityId_Code",
                table: "Beneficiaries");

            migrationBuilder.CreateSequence<int>(
                name: "BeneficiaryCodeSeq");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EnrollmentDate",
                table: "Beneficiaries",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<byte>(
                name: "MatchReviewStatus",
                table: "Beneficiaries",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<Guid>(
                name: "MergedIntoBeneficiaryId",
                table: "Beneficiaries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Beneficiaries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                table: "Beneficiaries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_Code",
                table: "Beneficiaries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_MergedIntoBeneficiaryId",
                table: "Beneficiaries",
                column: "MergedIntoBeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_RegistrationFacilityId_MatchReviewStatus",
                table: "Beneficiaries",
                columns: new[] { "RegistrationFacilityId", "MatchReviewStatus" });

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_Beneficiaries_MergedIntoBeneficiaryId",
                table: "Beneficiaries",
                column: "MergedIntoBeneficiaryId",
                principalTable: "Beneficiaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_Beneficiaries_MergedIntoBeneficiaryId",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_Code",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_MergedIntoBeneficiaryId",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_RegistrationFacilityId_MatchReviewStatus",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "EnrollmentDate",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "MatchReviewStatus",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "MergedIntoBeneficiaryId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Beneficiaries");

            migrationBuilder.DropSequence(
                name: "BeneficiaryCodeSeq");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_RegistrationFacilityId_Code",
                table: "Beneficiaries",
                columns: new[] { "RegistrationFacilityId", "Code" },
                unique: true);
        }
    }
}
