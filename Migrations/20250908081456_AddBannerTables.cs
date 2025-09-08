using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonAmour.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blog_Blog_Category_BlogCategoryCategoryId",
                table: "Blog");

            migrationBuilder.DropForeignKey(
                name: "FK_Blog_User_UserId",
                table: "Blog");

            migrationBuilder.DropForeignKey(
                name: "FK_Blog_Comment_User_UserId1",
                table: "Blog_Comment");

            migrationBuilder.DropIndex(
                name: "IX_Blog_Comment_UserId1",
                table: "Blog_Comment");

            migrationBuilder.DropIndex(
                name: "IX_Blog_BlogCategoryCategoryId",
                table: "Blog");

            migrationBuilder.DropIndex(
                name: "IX_Blog_UserId",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Blog_Comment");

            migrationBuilder.DropColumn(
                name: "BlogCategoryCategoryId",
                table: "Blog");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Blog");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Blog",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Excerpt",
                table: "Blog",
                newName: "excerpt");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Blog",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "ViewCount",
                table: "Blog",
                newName: "view_count");

            migrationBuilder.RenameColumn(
                name: "ReadTime",
                table: "Blog",
                newName: "read_time");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "Blog",
                newName: "is_published");

            migrationBuilder.RenameColumn(
                name: "IsFeatured",
                table: "Blog",
                newName: "is_featured");

            migrationBuilder.CreateTable(
                name: "BannerHomepage",
                columns: table => new
                {
                    banner_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    img_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_primary = table.Column<bool>(type: "bit", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerHomepage", x => x.banner_id);
                });

            migrationBuilder.CreateTable(
                name: "BannerProduct",
                columns: table => new
                {
                    banner_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    img_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_primary = table.Column<bool>(type: "bit", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerProduct", x => x.banner_id);
                });

            migrationBuilder.CreateTable(
                name: "BannerService",
                columns: table => new
                {
                    banner_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    img_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_primary = table.Column<bool>(type: "bit", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerService", x => x.banner_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerHomepage");

            migrationBuilder.DropTable(
                name: "BannerProduct");

            migrationBuilder.DropTable(
                name: "BannerService");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Blog",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "excerpt",
                table: "Blog",
                newName: "Excerpt");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Blog",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "view_count",
                table: "Blog",
                newName: "ViewCount");

            migrationBuilder.RenameColumn(
                name: "read_time",
                table: "Blog",
                newName: "ReadTime");

            migrationBuilder.RenameColumn(
                name: "is_published",
                table: "Blog",
                newName: "IsPublished");

            migrationBuilder.RenameColumn(
                name: "is_featured",
                table: "Blog",
                newName: "IsFeatured");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Blog_Comment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BlogCategoryCategoryId",
                table: "Blog",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Blog",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Comment_UserId1",
                table: "Blog_Comment",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_BlogCategoryCategoryId",
                table: "Blog",
                column: "BlogCategoryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_UserId",
                table: "Blog",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blog_Blog_Category_BlogCategoryCategoryId",
                table: "Blog",
                column: "BlogCategoryCategoryId",
                principalTable: "Blog_Category",
                principalColumn: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Blog_User_UserId",
                table: "Blog",
                column: "UserId",
                principalTable: "User",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Blog_Comment_User_UserId1",
                table: "Blog_Comment",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "user_id");
        }
    }
}
