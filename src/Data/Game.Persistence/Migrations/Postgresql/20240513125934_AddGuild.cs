#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class AddGuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "players",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    LeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    MaxMemberCount = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false)
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