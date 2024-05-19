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
                        OwnerId = table.Column<uint>(type: "int unsigned", nullable: false),
                        Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        Experience = table.Column<uint>(type: "int unsigned", nullable: false),
                        MaxMemberCount = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                        Gold = table.Column<uint>(type: "int unsigned", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Guilds", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Guilds_Players_OwnerId",
                            column: x => x.OwnerId,
                            principalTable: "Players",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "GuildRanks",
                    columns: table => new
                    {
                        GuildId = table.Column<uint>(type: "int unsigned", nullable: false),
                        Rank = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        Name = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Permissions = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_GuildRanks", x => new {x.GuildId, x.Rank});
                        table.ForeignKey(
                            name: "FK_GuildRanks_Guilds_GuildId",
                            column: x => x.GuildId,
                            principalTable: "Guilds",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "GuildMembers",
                    columns: table => new
                    {
                        GuildId = table.Column<uint>(type: "int unsigned", nullable: false),
                        PlayerId = table.Column<uint>(type: "int unsigned", nullable: false),
                        RankId = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        IsLeader = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        SpentExperience = table.Column<uint>(type: "int unsigned", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_GuildMembers", x => new {x.GuildId, x.PlayerId});
                        table.ForeignKey(
                            name: "FK_GuildMembers_GuildRanks_GuildId_RankId",
                            columns: x => new {x.GuildId, x.RankId},
                            principalTable: "GuildRanks",
                            principalColumns: new[] {"GuildId", "Rank"},
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_GuildMembers_Guilds_GuildId",
                            column: x => x.GuildId,
                            principalTable: "Guilds",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_GuildMembers_Players_PlayerId",
                            column: x => x.PlayerId,
                            principalTable: "Players",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: 1u,
                column: "GuildId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Players_GuildId",
                table: "Players",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_GuildId_RankId",
                table: "GuildMembers",
                columns: new[] {"GuildId", "RankId"});

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_PlayerId",
                table: "GuildMembers",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_OwnerId",
                table: "Guilds",
                column: "OwnerId");

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
                name: "GuildMembers");

            migrationBuilder.DropTable(
                name: "GuildRanks");

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