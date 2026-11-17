using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FacilityUsesAreaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) add the new column as nullable so existing rows can be backfilled first
            migrationBuilder.AddColumn<Guid>(
                name: "area_id",
                table: "facilities",
                type: "uuid",
                nullable: true);

            // 2) every facility keeps the area of the address it used to point to
            migrationBuilder.Sql("""
                UPDATE facilities
                SET area_id = a.area_id
                FROM addresses a
                WHERE facilities.address_id = a.id;
                """);

            // fail with a readable error instead of a generic NOT NULL violation if an address row is missing
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM facilities WHERE area_id IS NULL) THEN
                        RAISE EXCEPTION 'FacilityUsesAreaId: some facilities have an address_id with no matching address; area_id cannot be backfilled.';
                    END IF;
                END $$;
                """);

            // 3) enforce the new required column and its relationships
            migrationBuilder.AlterColumn<Guid>(
                name: "area_id",
                table: "facilities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_facilities_area_id",
                table: "facilities",
                column: "area_id");

            migrationBuilder.AddForeignKey(
                name: "fk_facilities_areas_area_id",
                table: "facilities",
                column: "area_id",
                principalTable: "areas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // 4) facilities no longer reference addresses
            migrationBuilder.DropForeignKey(
                name: "fk_facilities_addresses_address_id",
                table: "facilities");

            migrationBuilder.DropIndex(
                name: "ix_facilities_address_id",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "address_id",
                table: "facilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "address_id",
                table: "facilities",
                type: "uuid",
                nullable: true);

            // best effort: an area may contain several addresses, so pick a deterministic one
            migrationBuilder.Sql("""
                UPDATE facilities
                SET address_id = x.id
                FROM (
                    SELECT DISTINCT ON (a.area_id) a.area_id, a.id
                    FROM addresses a
                    ORDER BY a.area_id, a.id
                ) x
                WHERE facilities.area_id = x.area_id;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "address_id",
                table: "facilities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_facilities_address_id",
                table: "facilities",
                column: "address_id");

            migrationBuilder.AddForeignKey(
                name: "fk_facilities_addresses_address_id",
                table: "facilities",
                column: "address_id",
                principalTable: "addresses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "fk_facilities_areas_area_id",
                table: "facilities");

            migrationBuilder.DropIndex(
                name: "ix_facilities_area_id",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "area_id",
                table: "facilities");
        }
    }
}
