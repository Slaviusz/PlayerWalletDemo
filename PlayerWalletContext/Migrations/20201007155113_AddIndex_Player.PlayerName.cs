using Microsoft.EntityFrameworkCore.Migrations;

namespace PlayerWalletContext.Migrations
{
    public partial class AddIndex_PlayerPlayerName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Players_PlayerName",
                table: "Players",
                column: "PlayerName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_PlayerName",
                table: "Players");
        }
    }
}
