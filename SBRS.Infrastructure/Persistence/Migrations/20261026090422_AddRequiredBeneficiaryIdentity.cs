using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBRS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredBeneficiaryIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ValueRegex",
                table: "IdentityTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BeneficiaryIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BeneficiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeneficiaryIdentities_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiaryIdentities_IdentityTypes_IdentityTypeId",
                        column: x => x.IdentityTypeId,
                        principalTable: "IdentityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryIdentities_BeneficiaryId_IdentityTypeId",
                table: "BeneficiaryIdentities",
                columns: new[] { "BeneficiaryId", "IdentityTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryIdentities_IdentityTypeId_Value",
                table: "BeneficiaryIdentities",
                columns: new[] { "IdentityTypeId", "Value" });

            // The seeder was skipped: the types are already seeded, so the regex is filled here.
            // Upsert by Code: existing rows get the ValueRegex, missing rows are inserted with their names.
            // Each statement is its own batch so IF @@ROWCOUNT sees the surrounding UPDATE.
            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'FAMILY_BOOK';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'دفتر عائلة', N'Family book', 'FAMILY_BOOK', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'VACCINATION_CARD';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'بطاقة لقاحات', N'Vaccination card', 'VACCINATION_CARD', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[A-Za-z0-9/-]{1,20}$' WHERE Code = 'PASSPORT';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'جواز سفر', N'Passport', 'PASSPORT', '^[A-Za-z0-9/-]{1,20}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^\d{10}$' WHERE Code = 'NATIONAL_ID';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'هوية وطنية', N'National ID', 'NATIONAL_ID', '^\d{10}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'CIVIL_REGISTRY';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'قيد مدني', N'Civil registry', 'CIVIL_REGISTRY', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'DRIVING_LICENSE';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'شهادة قيادة', N'Driving license', 'DRIVING_LICENSE', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'BIRTH_CERTIFICATE';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'شهادة ميلاد', N'Birth certificate', 'BIRTH_CERTIFICATE', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'DISABILITY_CERTIFICATE';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'شهادة إعاقة', N'Disability certificate', 'DISABILITY_CERTIFICATE', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'FAMILY_STATEMENT';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'بيان عائلي', N'Family statement', 'FAMILY_STATEMENT', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^[\p{L}\p{N}/-]{1,30}$' WHERE Code = 'MARRIAGE_CERTIFICATE';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'وثيقة زواج', N'Marriage certificate', 'MARRIAGE_CERTIFICATE', '^[\p{L}\p{N}/-]{1,30}$', GETDATE());");

            migrationBuilder.Sql(@"UPDATE IdentityTypes SET ValueRegex = '^$' WHERE Code = 'UNREGISTERED';
IF @@ROWCOUNT = 0 INSERT INTO IdentityTypes (Id, ArName, EnName, Code, ValueRegex, CreatedAt) VALUES (NEWID(), N'غير مسجل', N'Unregistered', 'UNREGISTERED', '^$', GETDATE());");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeneficiaryIdentities");

            migrationBuilder.DropColumn(
                name: "ValueRegex",
                table: "IdentityTypes");
        }
    }
}
