#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Postgresql;

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
                PlayerId = table.Column<long>(type: "bigint", nullable: false),
                Slot = table.Column<byte>(type: "smallint", nullable: false),
                Type = table.Column<byte>(type: "smallint", nullable: false),
                Value = table.Column<long>(type: "bigint", nullable: false)
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
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PlayerQuickSlots");
    }
}