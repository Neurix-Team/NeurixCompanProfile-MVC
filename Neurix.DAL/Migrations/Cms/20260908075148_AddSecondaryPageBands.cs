using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class AddSecondaryPageBands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsPageBandItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BandKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LinkTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsPageBandItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsPageBandItems_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsPageBands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BandKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BadgeEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BadgeAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TitlePrefixEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TitlePrefixAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TitleHighlightEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TitleHighlightAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TitleSuffixEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TitleSuffixAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BodyEn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BodyAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ButtonTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButtonTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButtonUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ItemLinkTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemLinkTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmptyStateEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmptyStateAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsPageBands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsPageBands_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsPageBandItems_CompanyProfileId_PageKey_BandKey_DisplayOrder",
                table: "CmsPageBandItems",
                columns: new[] { "CompanyProfileId", "PageKey", "BandKey", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsPageBandItems_IsPublished",
                table: "CmsPageBandItems",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPageBands_CompanyProfileId_PageKey_BandKey",
                table: "CmsPageBands",
                columns: new[] { "CompanyProfileId", "PageKey", "BandKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsPageBands_IsPublished",
                table: "CmsPageBands",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsPageBandItems");

            migrationBuilder.DropTable(
                name: "CmsPageBands");
        }
    }
}
