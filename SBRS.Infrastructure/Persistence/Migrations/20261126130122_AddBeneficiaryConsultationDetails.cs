using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaryConsultationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beneficiary_consultation_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    beneficiary_consultation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    disease_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beneficiary_consultation_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_beneficiary_consultation_details_beneficiary_consultations_",
                        column: x => x.beneficiary_consultation_id,
                        principalTable: "beneficiary_consultations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_consultation_details_diseases_disease_id",
                        column: x => x.disease_id,
                        principalTable: "diseases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_consultation_details_beneficiary_consultation_i",
                table: "beneficiary_consultation_details",
                columns: new[] { "beneficiary_consultation_id", "disease_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_consultation_details_disease_id",
                table: "beneficiary_consultation_details",
                column: "disease_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beneficiary_consultation_details");
        }
    }
}
