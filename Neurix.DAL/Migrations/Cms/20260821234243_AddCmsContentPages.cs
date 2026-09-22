using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class AddCmsContentPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsContentPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MetaDescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetaDescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HeroBadgeEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    HeroBadgeAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    HeroTitlePrefixEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HeroTitlePrefixAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HeroTitleHighlightEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HeroTitleHighlightAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HeroSubtitleEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HeroSubtitleAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BodyEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MissionTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MissionTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MissionTextEn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    MissionTextAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    VisionTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VisionTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VisionTextEn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    VisionTextAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CtaBadgeEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CtaBadgeAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CtaTitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CtaTitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CtaSubtitleEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CtaSubtitleAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CtaButtonTextEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CtaButtonTextAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ExtraDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsContentPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsContentPages_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentPages_CompanyProfileId",
                table: "CmsContentPages",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentPages_CompanyProfileId_Slug",
                table: "CmsContentPages",
                columns: new[] { "CompanyProfileId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentPages_IsPublished",
                table: "CmsContentPages",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsContentPages");
        }
    }
}
