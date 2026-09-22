using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class AddCmsCtaSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsCtaSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BadgeEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BadgeAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitlePrefixEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ButtonTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ButtonTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ButtonUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BackgroundImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsCtaSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsCtaSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsCtaSections_CompanyProfileId",
                table: "CmsCtaSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsCtaSections_IsPublished",
                table: "CmsCtaSections",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsCtaSections");
        }
    }
}
