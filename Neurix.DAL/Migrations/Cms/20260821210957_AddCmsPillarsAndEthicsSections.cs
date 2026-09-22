using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class AddCmsPillarsAndEthicsSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsEthicsSections",
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
                    DescriptionEn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TopImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TopImageAltEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TopImageAltAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BottomImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BottomImageAltEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BottomImageAltAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BackgroundImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsEthicsSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsEthicsSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsPillarItems",
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
                    table.PrimaryKey("PK_CmsPillarItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsPillarItems_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsPillarsSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubtitleEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SubtitleAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsPillarsSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsPillarsSections_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsEthicsSections_CompanyProfileId",
                table: "CmsEthicsSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsEthicsSections_IsPublished",
                table: "CmsEthicsSections",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPillarItems_CompanyProfileId_DisplayOrder",
                table: "CmsPillarItems",
                columns: new[] { "CompanyProfileId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsPillarItems_IsPublished",
                table: "CmsPillarItems",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPillarsSections_CompanyProfileId",
                table: "CmsPillarsSections",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsPillarsSections_IsPublished",
                table: "CmsPillarsSections",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsEthicsSections");

            migrationBuilder.DropTable(
                name: "CmsPillarItems");

            migrationBuilder.DropTable(
                name: "CmsPillarsSections");
        }
    }
}
