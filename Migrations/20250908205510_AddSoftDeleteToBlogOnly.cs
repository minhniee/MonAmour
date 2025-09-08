using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonAmour.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToBlogOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "Blog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "Blog",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "Blog");
        }
    }
}
