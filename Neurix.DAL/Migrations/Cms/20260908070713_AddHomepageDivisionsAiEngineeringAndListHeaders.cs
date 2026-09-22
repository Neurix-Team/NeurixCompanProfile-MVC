using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class AddHomepageDivisionsAiEngineeringAndListHeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsAiEngineeringItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsAiEngineeringItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsAiEngineeringItems_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsAiEngineeringSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BadgeEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BadgeAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsAiEngineeringSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsAiEngineeringSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsDivisionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LinkTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LinkTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsDivisionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsDivisionItems_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsDivisionsSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BadgeEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BadgeAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsDivisionsSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsDivisionsSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsListSectionHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BadgeEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BadgeAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubtitleEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubtitleAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ItemLinkTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemLinkTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultCategoryLabelEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultCategoryLabelAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReadTimeSuffixEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReadTimeSuffixAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UndatedLabelEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UndatedLabelAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButtonTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButtonTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButtonUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsListSectionHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsListSectionHeaders_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsAiEngineeringItems_CompanyProfileId_DisplayOrder",
                table: "CmsAiEngineeringItems",
                columns: new[] { "CompanyProfileId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsAiEngineeringItems_IsPublished",
                table: "CmsAiEngineeringItems",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsAiEngineeringSections_CompanyProfileId",
                table: "CmsAiEngineeringSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsAiEngineeringSections_IsPublished",
                table: "CmsAiEngineeringSections",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsDivisionItems_CompanyProfileId_DisplayOrder",
                table: "CmsDivisionItems",
                columns: new[] { "CompanyProfileId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsDivisionItems_IsPublished",
                table: "CmsDivisionItems",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsDivisionsSections_CompanyProfileId",
                table: "CmsDivisionsSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsDivisionsSections_IsPublished",
                table: "CmsDivisionsSections",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsListSectionHeaders_CompanyProfileId_SectionKey",
                table: "CmsListSectionHeaders",
                columns: new[] { "CompanyProfileId", "SectionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsListSectionHeaders_IsPublished",
                table: "CmsListSectionHeaders",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsAiEngineeringItems");

            migrationBuilder.DropTable(
                name: "CmsAiEngineeringSections");

            migrationBuilder.DropTable(
                name: "CmsDivisionItems");

            migrationBuilder.DropTable(
                name: "CmsDivisionsSections");

            migrationBuilder.DropTable(
                name: "CmsListSectionHeaders");
        }
    }
}
