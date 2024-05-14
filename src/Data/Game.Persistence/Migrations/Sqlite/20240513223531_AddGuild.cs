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
                table: "Players",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    LeaderId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    Experience = table.Column<uint>(type: "INTEGER", nullable: false),
                    MaxMemberCount = table.Column<ushort>(type: "INTEGER", nullable: false),
                    Gold = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guilds_Players_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_GuildId",
                table: "Players",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_LeaderId",
                table: "Guilds",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Guilds_GuildId",
                table: "Players",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Guilds_GuildId",
                table: "Players");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_Players_GuildId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Players");
        }
    }
}