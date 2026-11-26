using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiseaseCategoriesAndDiseases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "disease_categories",
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
                    table.PrimaryKey("pk_disease_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "diseases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    en_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    disease_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_diseases", x => x.id);
                    table.ForeignKey(
                        name: "fk_diseases_disease_categories_disease_category_id",
                        column: x => x.disease_category_id,
                        principalTable: "disease_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_disease_categories_code",
                table: "disease_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_diseases_code",
                table: "diseases",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_diseases_disease_category_id",
                table: "diseases",
                column: "disease_category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diseases");

            migrationBuilder.DropTable(
                name: "disease_categories");
        }
    }
}
