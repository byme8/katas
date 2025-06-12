using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pagination.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_IsDeleted_UserId_CreatedAt",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IsDeleted_UserId_Id",
                table: "Comments",
                columns: new[] { "IsDeleted", "UserId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_IsDeleted_UserId_Id",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IsDeleted_UserId_CreatedAt",
                table: "Comments",
                columns: new[] { "IsDeleted", "UserId", "CreatedAt" });
        }
    }
}
