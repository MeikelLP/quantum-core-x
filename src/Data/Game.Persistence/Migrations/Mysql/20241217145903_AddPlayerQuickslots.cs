#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class AddPlayerQuickslots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                    name: "PlayerQuickSlots",
                    columns: table => new
                    {
                        PlayerId = table.Column<uint>(type: "int unsigned", nullable: false),
                        Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        Value = table.Column<uint>(type: "int unsigned", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_PlayerQuickSlots", x => new {x.PlayerId, x.Slot});
                        table.ForeignKey(
                            name: "FK_PlayerQuickSlots_Players_PlayerId",
                            column: x => x.PlayerId,
                            principalTable: "Players",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerQuickSlots");
        }
    }
}
