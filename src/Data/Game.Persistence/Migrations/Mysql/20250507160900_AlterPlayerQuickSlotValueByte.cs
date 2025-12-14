#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Mysql;

/// <inheritdoc />
public partial class AlterPlayerQuickSlotValueByte : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<byte>(
            name: "Value",
            table: "PlayerQuickSlots",
            type: "tinyint unsigned",
            nullable: false,
            oldClrType: typeof(uint),
            oldType: "int unsigned");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<uint>(
            name: "Value",
            table: "PlayerQuickSlots",
            type: "int unsigned",
            nullable: false,
            oldClrType: typeof(byte),
            oldType: "tinyint unsigned");
    }
}