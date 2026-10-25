using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCityIdToAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- Part A: Areas.CityId ------------------------------------------------

            // 1) add nullable first, so the backfill does not hit the FK with an empty Guid
            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "Areas",
                type: "uniqueidentifier",
                nullable: true);

            // 2) backfill through the existing District -> Region -> City chain
            migrationBuilder.Sql(@"
UPDATE a
SET a.CityId = r.CityId
FROM Areas a
INNER JOIN Districts d ON d.Id = a.DistrictId
INNER JOIN Regions r ON r.Id = d.RegionId;");

            // 3) then make it NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "CityId",
                table: "Areas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // 4) index and FK only after the backfill and the AlterColumn
            migrationBuilder.CreateIndex(
                name: "IX_Areas_CityId_ArName",
                table: "Areas",
                columns: new[] { "CityId", "ArName" });

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Cities_CityId",
                table: "Areas",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ---- Part B: Beneficiaries.AreaId (table is empty, no backfill needed) ----

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "Beneficiaries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_AreaId",
                table: "Beneficiaries",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_Areas_AreaId",
                table: "Beneficiaries",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_Areas_AreaId",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_AreaId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Beneficiaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Cities_CityId",
                table: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Areas_CityId_ArName",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Areas");
        }
    }
}
