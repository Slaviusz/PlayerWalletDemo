using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlayerWalletContext.Migrations
{
    public partial class Add_WalletLogEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletLogs",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(nullable: false),
                    WalletId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ResultType = table.Column<int>(nullable: false),
                    Memento = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletLogs", x => new { x.TransactionId, x.WalletId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletLogs");
        }
    }
}
