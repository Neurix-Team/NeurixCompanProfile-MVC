using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations.Cms
{
    /// <inheritdoc />
    public partial class UpdateCmsContentPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "CmsContentPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CtaButtonUrl",
                table: "CmsContentPages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "CmsContentPages");

            migrationBuilder.DropColumn(
                name: "CtaButtonUrl",
                table: "CmsContentPages");
        }
    }
}
