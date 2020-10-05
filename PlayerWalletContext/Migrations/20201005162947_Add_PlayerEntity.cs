using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlayerWalletContext.Migrations
{
    public partial class Add_PlayerEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    PlayerName = table.Column<string>(maxLength: 32, nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "Active", "PlayerName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), true, "NicknameJim" });

            migrationBuilder.CreateIndex(
                name: "IX_Players_Active",
                table: "Players",
                column: "Active");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
