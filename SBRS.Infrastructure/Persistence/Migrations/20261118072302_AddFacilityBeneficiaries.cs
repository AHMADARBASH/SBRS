using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityBeneficiaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "facility_beneficiaries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    beneficiary_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrolled_on = table.Column<DateOnly>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facility_beneficiaries", x => x.id);
                    table.ForeignKey(
                        name: "fk_facility_beneficiaries_beneficiaries_beneficiary_id",
                        column: x => x.beneficiary_id,
                        principalTable: "beneficiaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_facility_beneficiaries_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_facility_beneficiaries_beneficiary_id",
                table: "facility_beneficiaries",
                column: "beneficiary_id");

            migrationBuilder.CreateIndex(
                name: "ix_facility_beneficiaries_facility_id_beneficiary_id",
                table: "facility_beneficiaries",
                columns: new[] { "facility_id", "beneficiary_id" },
                unique: true);

            // Backfill: every existing beneficiary was enrolled at the facility where he was first
            // registered. EnrolledOn follows the beneficiary's original EnrollmentDate; audit data
            // is copied. NOT EXISTS keeps the statement idempotent (no duplicate enrollment rows).
            migrationBuilder.Sql("""
                INSERT INTO facility_beneficiaries
                    (id, beneficiary_id, facility_id, enrolled_on, is_active, created_at, created_by)
                SELECT gen_random_uuid(), b.id, b.registration_facility_id, b.enrollment_date,
                       TRUE, b.created_at, b.created_by
                FROM beneficiaries AS b
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM facility_beneficiaries AS fb
                    WHERE fb.beneficiary_id = b.id
                      AND fb.facility_id = b.registration_facility_id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "facility_beneficiaries");
        }
    }
}
