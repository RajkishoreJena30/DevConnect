using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevConnect.Migrations.DevConnectDb
{
    /// <inheritdoc />
    public partial class oidcfieldsaddedtouser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderUserId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProviderUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Posts");
        }
    }
}
