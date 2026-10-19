using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FacilityTypeId",
                table: "Facilities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TypeId",
                table: "Facilities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "FacilityTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_FacilityTypeId",
                table: "Facilities",
                column: "FacilityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_TypeId",
                table: "Facilities",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityTypes_Code",
                table: "FacilityTypes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Facilities_FacilityTypes_FacilityTypeId",
                table: "Facilities",
                column: "FacilityTypeId",
                principalTable: "FacilityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Facilities_FacilityTypes_TypeId",
                table: "Facilities",
                column: "TypeId",
                principalTable: "FacilityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facilities_FacilityTypes_FacilityTypeId",
                table: "Facilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Facilities_FacilityTypes_TypeId",
                table: "Facilities");

            migrationBuilder.DropTable(
                name: "FacilityTypes");

            migrationBuilder.DropIndex(
                name: "IX_Facilities_FacilityTypeId",
                table: "Facilities");

            migrationBuilder.DropIndex(
                name: "IX_Facilities_TypeId",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "FacilityTypeId",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Facilities");
        }
    }
}
