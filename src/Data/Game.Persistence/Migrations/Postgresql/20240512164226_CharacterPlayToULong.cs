using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumCore.Game.Persistence.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class CharacterPlayToULong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PlayTime",
                table: "players",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "PlayTime",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }
    }
}
