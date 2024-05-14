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
                table: "Players",
                type: "int unsigned",
                nullable: true);

            migrationBuilder.CreateTable(
                    name: "Guilds",
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
                        LeaderId = table.Column<uint>(type: "int unsigned", nullable: false),
                        Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        Experience = table.Column<uint>(type: "int unsigned", nullable: false),
                        MaxMemberCount = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                        Gold = table.Column<uint>(type: "int unsigned", nullable: false)
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
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

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