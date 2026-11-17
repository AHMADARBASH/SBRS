using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_branches_organization_id",
                table: "branches");

            // 1) add the new column as nullable so existing rows can be backfilled first
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "branches",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            // 2) every existing branch needs a code before NOT NULL and the unique index apply.
            // There is no safe way to derive the real code from old data, so each branch gets a
            // unique, letters-only placeholder; the branch seeder replaces the codes it knows.
            migrationBuilder.Sql("""
                WITH numbered AS (
                    SELECT id, row_number() OVER (PARTITION BY organization_id ORDER BY created_at, id) AS rn
                    FROM branches
                )
                UPDATE branches b
                SET code = 'BR' || translate(n.rn::text, '0123456789', 'ABCDEFGHIJ')
                FROM numbered n
                WHERE b.id = n.id;
                """);

            // 3) enforce the required column and its per-organization uniqueness
            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "branches",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_organization_id_code",
                table: "branches",
                columns: new[] { "organization_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_branches_organization_id_code",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "code",
                table: "branches");

            migrationBuilder.CreateIndex(
                name: "ix_branches_organization_id",
                table: "branches",
                column: "organization_id");
        }
    }
}
