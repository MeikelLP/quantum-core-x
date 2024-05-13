#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddGuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "GuildId",
                table: "players",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    LeaderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    Experience = table.Column<uint>(type: "INTEGER", nullable: false),
                    MaxMemberCount = table.Column<ushort>(type: "INTEGER", nullable: false),
                    Gold = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guilds_players_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "players",
                keyColumn: "Id",
                keyValue: new Guid("fefa4396-c5d1-4d7f-bc84-5df40867eac8"),
                column: "GuildId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_players_GuildId",
                table: "players",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_guilds_LeaderId",
                table: "guilds",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_players_guilds_GuildId",
                table: "players",
                column: "GuildId",
                principalTable: "guilds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_guilds_GuildId",
                table: "players");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropIndex(
                name: "IX_players_GuildId",
                table: "players");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "players");
        }
    }
}