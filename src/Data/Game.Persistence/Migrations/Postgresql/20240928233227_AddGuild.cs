﻿#nullable disable

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
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    MaxMemberCount = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "GuildNews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    GuildId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildNews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildNews_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GuildNews_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildRanks",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Permissions = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildRanks", x => new {x.GuildId, x.Position});
                    table.ForeignKey(
                        name: "FK_GuildRanks_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildMembers",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    RankPosition = table.Column<byte>(type: "smallint", nullable: false),
                    IsLeader = table.Column<bool>(type: "boolean", nullable: false),
                    SpentExperience = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildMembers", x => new {x.GuildId, x.PlayerId});
                    table.ForeignKey(
                        name: "FK_GuildMembers_GuildRanks_GuildId_RankPosition",
                        columns: x => new {x.GuildId, x.RankPosition},
                        principalTable: "GuildRanks",
                        principalColumns: new[] {"GuildId", "Position"},
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
                });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: 1L,
                column: "GuildId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Players_GuildId",
                table: "Players",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_GuildId_RankPosition",
                table: "GuildMembers",
                columns: new[] {"GuildId", "RankPosition"});

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_PlayerId",
                table: "GuildMembers",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuildNews_GuildId",
                table: "GuildNews",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildNews_PlayerId",
                table: "GuildNews",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_OwnerId",
                table: "Guilds",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Guilds_GuildId",
                table: "Players",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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
                name: "GuildNews");

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