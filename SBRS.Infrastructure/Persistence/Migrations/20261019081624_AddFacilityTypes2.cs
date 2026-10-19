using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityTypes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facilities_FacilityTypes_FacilityTypeId",
                table: "Facilities");

            migrationBuilder.DropIndex(
                name: "IX_Facilities_FacilityTypeId",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "FacilityTypeId",
                table: "Facilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FacilityTypeId",
                table: "Facilities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_FacilityTypeId",
                table: "Facilities",
                column: "FacilityTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facilities_FacilityTypes_FacilityTypeId",
                table: "Facilities",
                column: "FacilityTypeId",
                principalTable: "FacilityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
