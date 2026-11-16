using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "facilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "facility_categories",
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
                    table.PrimaryKey("pk_facility_categories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_facilities_category_id",
                table: "facilities",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_facility_categories_code",
                table: "facility_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_facility_categories_en_name",
                table: "facility_categories",
                column: "en_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_facilities_facility_categories_category_id",
                table: "facilities",
                column: "category_id",
                principalTable: "facility_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_facilities_facility_categories_category_id",
                table: "facilities");

            migrationBuilder.DropTable(
                name: "facility_categories");

            migrationBuilder.DropIndex(
                name: "ix_facilities_category_id",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "facilities");
        }
    }
}
