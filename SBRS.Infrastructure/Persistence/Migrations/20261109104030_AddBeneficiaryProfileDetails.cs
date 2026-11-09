using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBeneficiaryProfileDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beneficiary_statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    en_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beneficiary_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chronic_diseases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    en_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chronic_diseases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "disability_reasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    en_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disability_reasons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "education_issues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    en_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_education_issues", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "marital_statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    en_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_marital_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "beneficiary_chronic_diseases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    beneficiary_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chronic_disease_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beneficiary_chronic_diseases", x => x.id);
                    table.ForeignKey(
                        name: "fk_beneficiary_chronic_diseases_beneficiaries_beneficiary_id",
                        column: x => x.beneficiary_id,
                        principalTable: "beneficiaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_chronic_diseases_chronic_diseases_chronic_disea",
                        column: x => x.chronic_disease_id,
                        principalTable: "chronic_diseases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "beneficiary_disabilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    beneficiary_id = table.Column<Guid>(type: "uuid", nullable: false),
                    disability_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beneficiary_disabilities", x => x.id);
                    table.ForeignKey(
                        name: "fk_beneficiary_disabilities_beneficiaries_beneficiary_id",
                        column: x => x.beneficiary_id,
                        principalTable: "beneficiaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_disabilities_disability_reasons_reason_id",
                        column: x => x.reason_id,
                        principalTable: "disability_reasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_disabilities_disability_types_disability_type_id",
                        column: x => x.disability_type_id,
                        principalTable: "disability_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "beneficiary_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    beneficiary_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number1 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone_number2 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_displaced = table.Column<bool>(type: "boolean", nullable: false),
                    displacement_date = table.Column<DateOnly>(type: "date", nullable: true),
                    old_area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    accommodation_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    beneficiary_status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_work = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    previous_work = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    education_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    education_issue_id = table.Column<Guid>(type: "uuid", nullable: true),
                    marital_status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wives_number = table.Column<short>(type: "smallint", nullable: true),
                    children_number = table.Column<short>(type: "smallint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beneficiary_profiles", x => x.id);
                    table.CheckConstraint("ck_beneficiary_profiles_children_number_range", "children_number IS NULL OR children_number BETWEEN 0 AND 30");
                    table.CheckConstraint("ck_beneficiary_profiles_displacement_consistency", "(is_displaced = true) OR (displacement_date IS NULL AND old_area_id IS NULL)");
                    table.CheckConstraint("ck_beneficiary_profiles_wives_number_range", "wives_number IS NULL OR wives_number BETWEEN 1 AND 4");
                    table.ForeignKey(
                        name: "fk_beneficiary_profiles_accommodation_types_accommodation_type",
                        column: x => x.accommodation_type_id,
                        principalTable: "accommodation_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_profiles_areas_old_area_id",
                        column: x => x.old_area_id,
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_profiles_beneficiaries_beneficiary_id",
                        column: x => x.beneficiary_id,
                        principalTable: "beneficiaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_profiles_beneficiary_statuses_beneficiary_statu",
                        column: x => x.beneficiary_status_id,
                        principalTable: "beneficiary_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_profiles_education_issues_education_issue_id",
                        column: x => x.education_issue_id,
                        principalTable: "education_issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_profiles_education_types_education_type_id",
                        column: x => x.education_type_id,
                        principalTable: "education_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_beneficiary_profiles_marital_statuses_marital_status_id",
                        column: x => x.marital_status_id,
                        principalTable: "marital_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_chronic_diseases_beneficiary_id_chronic_disease",
                table: "beneficiary_chronic_diseases",
                columns: new[] { "beneficiary_id", "chronic_disease_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_chronic_diseases_chronic_disease_id",
                table: "beneficiary_chronic_diseases",
                column: "chronic_disease_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_disabilities_beneficiary_id_disability_type_id",
                table: "beneficiary_disabilities",
                columns: new[] { "beneficiary_id", "disability_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_disabilities_disability_type_id",
                table: "beneficiary_disabilities",
                column: "disability_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_disabilities_reason_id",
                table: "beneficiary_disabilities",
                column: "reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_profiles_accommodation_type_id",
                table: "beneficiary_profiles",
                column: "accommodation_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_profiles_beneficiary_id",
                table: "beneficiary_profiles",
                column: "beneficiary_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_profiles_beneficiary_status_id",
                table: "beneficiary_profiles",
                column: "beneficiary_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_profiles_education_issue_id",
                table: "beneficiary_profiles",
                column: "education_issue_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_profiles_education_type_id",
                table: "beneficiary_profiles",
                column: "education_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_profiles_marital_status_id",
                table: "beneficiary_profiles",
                column: "marital_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_profiles_old_area_id",
                table: "beneficiary_profiles",
                column: "old_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_statuses_code",
                table: "beneficiary_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_beneficiary_statuses_en_name",
                table: "beneficiary_statuses",
                column: "en_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chronic_diseases_code",
                table: "chronic_diseases",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chronic_diseases_en_name",
                table: "chronic_diseases",
                column: "en_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_disability_reasons_code",
                table: "disability_reasons",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_disability_reasons_en_name",
                table: "disability_reasons",
                column: "en_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_education_issues_code",
                table: "education_issues",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_education_issues_en_name",
                table: "education_issues",
                column: "en_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_marital_statuses_code",
                table: "marital_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_marital_statuses_en_name",
                table: "marital_statuses",
                column: "en_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beneficiary_chronic_diseases");

            migrationBuilder.DropTable(
                name: "beneficiary_disabilities");

            migrationBuilder.DropTable(
                name: "beneficiary_profiles");

            migrationBuilder.DropTable(
                name: "chronic_diseases");

            migrationBuilder.DropTable(
                name: "disability_reasons");

            migrationBuilder.DropTable(
                name: "beneficiary_statuses");

            migrationBuilder.DropTable(
                name: "education_issues");

            migrationBuilder.DropTable(
                name: "marital_statuses");
        }
    }
}
