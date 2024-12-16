using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProniaApplication.Migrations
{
    /// <inheritdoc />
    public partial class CreatestringId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_AspNetUsers_AppUserId",
                table: "BasketItems");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "BasketItems",
                newName: "AppUserID");

            migrationBuilder.RenameIndex(
                name: "IX_BasketItems_AppUserId",
                table: "BasketItems",
                newName: "IX_BasketItems_AppUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_AspNetUsers_AppUserID",
                table: "BasketItems",
                column: "AppUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_AspNetUsers_AppUserID",
                table: "BasketItems");

            migrationBuilder.RenameColumn(
                name: "AppUserID",
                table: "BasketItems",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_BasketItems_AppUserID",
                table: "BasketItems",
                newName: "IX_BasketItems_AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_AspNetUsers_AppUserId",
                table: "BasketItems",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
