using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurix.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConvertedAtUtc",
                table: "Leads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConvertedToCompanyId",
                table: "Leads",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConvertedToContactId",
                table: "Leads",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedToCompanyId",
                table: "Leads",
                column: "ConvertedToCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedToContactId",
                table: "Leads",
                column: "ConvertedToContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Companies_ConvertedToCompanyId",
                table: "Leads",
                column: "ConvertedToCompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Contacts_ConvertedToContactId",
                table: "Leads",
                column: "ConvertedToContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Companies_ConvertedToCompanyId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Contacts_ConvertedToContactId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedToCompanyId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedToContactId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedAtUtc",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedToCompanyId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ConvertedToContactId",
                table: "Leads");
        }
    }
}
