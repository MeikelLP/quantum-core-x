#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Mysql
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
                type: "int unsigned",
                nullable: true);

            migrationBuilder.CreateTable(
                    name: "guilds",
                    columns: table => new
                    {
                        Id = table.Column<uint>(type: "int unsigned", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        Name = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        LeaderId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        Experience = table.Column<uint>(type: "int unsigned", nullable: false),
                        MaxMemberCount = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                        Gold = table.Column<uint>(type: "int unsigned", nullable: false)
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
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

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