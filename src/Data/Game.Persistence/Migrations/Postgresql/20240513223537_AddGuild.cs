#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                table: "Players",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    LeaderId = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    MaxMemberCount = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false)
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