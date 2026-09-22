using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class AddFullCmsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsBlogPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SummaryEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SummaryAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BodyEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuthorNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuthorNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReadTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsBlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsBlogPosts_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsDivisionPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeroTitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HeroTitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HeroSubtitleEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HeroSubtitleAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MissionEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MissionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsDivisionPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsDivisionPages_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsMediaAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AltTextEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AltTextAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsMediaAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsMediaAssets_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SummaryEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SummaryAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TechnologiesUsed = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProjectUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GithubUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsProjects_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsSiteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValueEn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ValueAr = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SettingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsSiteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsSiteSettings_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsTeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BioEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BioAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsTeamMembers_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsTestimonials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuthorNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuthorTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuthorTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuthorPhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QuoteEn = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    QuoteAr = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsTestimonials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsTestimonials_CmsCompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CmsCompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlogPosts_CompanyProfileId",
                table: "CmsBlogPosts",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlogPosts_CompanyProfileId_Slug",
                table: "CmsBlogPosts",
                columns: new[] { "CompanyProfileId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlogPosts_IsFeatured",
                table: "CmsBlogPosts",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlogPosts_IsPublished",
                table: "CmsBlogPosts",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlogPosts_PublishedAtUtc",
                table: "CmsBlogPosts",
                column: "PublishedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CmsDivisionPages_CompanyProfileId",
                table: "CmsDivisionPages",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsDivisionPages_CompanyProfileId_Slug",
                table: "CmsDivisionPages",
                columns: new[] { "CompanyProfileId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsDivisionPages_IsPublished",
                table: "CmsDivisionPages",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsMediaAssets_Category",
                table: "CmsMediaAssets",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CmsMediaAssets_CompanyProfileId",
                table: "CmsMediaAssets",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsMediaAssets_CreatedAtUtc",
                table: "CmsMediaAssets",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CmsProjects_CompanyProfileId",
                table: "CmsProjects",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsProjects_CompanyProfileId_Slug",
                table: "CmsProjects",
                columns: new[] { "CompanyProfileId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsProjects_DisplayOrder",
                table: "CmsProjects",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CmsProjects_IsFeatured",
                table: "CmsProjects",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_CmsProjects_IsPublished",
                table: "CmsProjects",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsSiteSettings_CompanyProfileId",
                table: "CmsSiteSettings",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsSiteSettings_CompanyProfileId_Key",
                table: "CmsSiteSettings",
                columns: new[] { "CompanyProfileId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsSiteSettings_GroupName",
                table: "CmsSiteSettings",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_CmsTeamMembers_CompanyProfileId",
                table: "CmsTeamMembers",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsTeamMembers_DisplayOrder",
                table: "CmsTeamMembers",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CmsTeamMembers_IsPublished",
                table: "CmsTeamMembers",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_CmsTestimonials_CompanyProfileId",
                table: "CmsTestimonials",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsTestimonials_DisplayOrder",
                table: "CmsTestimonials",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CmsTestimonials_IsFeatured",
                table: "CmsTestimonials",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_CmsTestimonials_IsPublished",
                table: "CmsTestimonials",
                column: "IsPublished");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsBlogPosts");

            migrationBuilder.DropTable(
                name: "CmsDivisionPages");

            migrationBuilder.DropTable(
                name: "CmsMediaAssets");

            migrationBuilder.DropTable(
                name: "CmsProjects");

            migrationBuilder.DropTable(
                name: "CmsSiteSettings");

            migrationBuilder.DropTable(
                name: "CmsTeamMembers");

            migrationBuilder.DropTable(
                name: "CmsTestimonials");
        }
    }
}
