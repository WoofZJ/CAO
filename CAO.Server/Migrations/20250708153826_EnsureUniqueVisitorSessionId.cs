using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAO.Server.Migrations
{
    /// <inheritdoc />
    public partial class EnsureUniqueVisitorSessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VisitorInfos_SessionId",
                table: "VisitorInfos",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisitorInfos_SessionId",
                table: "VisitorInfos");
        }
    }
}
