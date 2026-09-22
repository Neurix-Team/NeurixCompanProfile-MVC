using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class AddCmsHomeSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsHeroSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BadgeEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BadgeAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleSuffixEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleHighlightAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleSuffixAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubtitleEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SubtitleAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PrimaryButtonTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimaryButtonTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimaryButtonUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SecondaryButtonTextEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecondaryButtonTextAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecondaryButtonUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Stat1Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Stat1LabelEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stat1LabelAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stat2Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Stat2LabelEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stat2LabelAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stat3Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Stat3LabelEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stat3LabelAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsHeroSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsHeroSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsHumanVisionSections",
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
                    Paragraph1En = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Paragraph1Ar = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Paragraph2En = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Paragraph2Ar = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageAltEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageAltAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsHumanVisionSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsHumanVisionSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsPioneersSections",
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
                    Paragraph1En = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Paragraph1Ar = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Paragraph2En = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Paragraph2Ar = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageAltEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageAltAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsPioneersSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsPioneersSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsHeroSections_CompanyProfileId",
                table: "CmsHeroSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsHeroSections_IsPublished",
                table: "CmsHeroSections",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsHumanVisionSections_CompanyProfileId",
                table: "CmsHumanVisionSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsHumanVisionSections_IsPublished",
                table: "CmsHumanVisionSections",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPioneersSections_CompanyProfileId",
                table: "CmsPioneersSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsPioneersSections_IsPublished",
                table: "CmsPioneersSections",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsHeroSections");

            migrationBuilder.DropTable(
                name: "CmsHumanVisionSections");

            migrationBuilder.DropTable(
                name: "CmsPioneersSections");
        }
    }
}
