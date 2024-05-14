using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumCore.Game.Persistence.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class CharacterPlayToULong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "PlayTime",
                table: "Players",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "PlayTime",
                table: "Players",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");
        }
    }
}
