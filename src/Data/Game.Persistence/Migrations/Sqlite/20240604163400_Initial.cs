﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using QuantumCore.API.Game.Types;

#nullable disable

namespace QuantumCore.Game.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeletedPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerClass = table.Column<byte>(type: "INTEGER", nullable: false),
                    SkillGroup = table.Column<byte>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false),
                    Gold = table.Column<byte>(type: "INTEGER", nullable: false),
                    St = table.Column<byte>(type: "INTEGER", nullable: false),
                    Ht = table.Column<byte>(type: "INTEGER", nullable: false),
                    Dx = table.Column<byte>(type: "INTEGER", nullable: false),
                    Iq = table.Column<byte>(type: "INTEGER", nullable: false),
                    PositionX = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionY = table.Column<int>(type: "INTEGER", nullable: false),
                    Health = table.Column<long>(type: "INTEGER", nullable: false),
                    Mana = table.Column<long>(type: "INTEGER", nullable: false),
                    Stamina = table.Column<long>(type: "INTEGER", nullable: false),
                    BodyPart = table.Column<int>(type: "INTEGER", nullable: false),
                    HairPart = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    Empire = table.Column<byte>(type: "INTEGER", nullable: false),
                    PlayerClass = table.Column<byte>(type: "INTEGER", nullable: false),
                    SkillGroup = table.Column<byte>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    Experience = table.Column<uint>(type: "INTEGER", nullable: false),
                    Gold = table.Column<uint>(type: "INTEGER", nullable: false),
                    St = table.Column<byte>(type: "INTEGER", nullable: false),
                    Ht = table.Column<byte>(type: "INTEGER", nullable: false),
                    Dx = table.Column<byte>(type: "INTEGER", nullable: false),
                    Iq = table.Column<byte>(type: "INTEGER", nullable: false),
                    PositionX = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionY = table.Column<int>(type: "INTEGER", nullable: false),
                    Health = table.Column<long>(type: "INTEGER", nullable: false),
                    Mana = table.Column<long>(type: "INTEGER", nullable: false),
                    Stamina = table.Column<long>(type: "INTEGER", nullable: false),
                    BodyPart = table.Column<uint>(type: "INTEGER", nullable: false),
                    HairPart = table.Column<uint>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    GivenStatusPoints = table.Column<uint>(type: "INTEGER", nullable: false),
                    AvailableStatusPoints = table.Column<uint>(type: "INTEGER", nullable: false),
                    AvailableSkillPoints = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    PlayerId = table.Column<uint>(type: "INTEGER", nullable: false),
                    SkillId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ReadsRequired = table.Column<uint>(type: "INTEGER", nullable: false),
                    MasterType = table.Column<byte>(type: "INTEGER", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    NextReadTime = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Command = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_PermissionGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "PermissionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Window = table.Column<byte>(type: "INTEGER", nullable: false),
                    Position = table.Column<uint>(type: "INTEGER", nullable: false),
                    Count = table.Column<byte>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionUsers",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionUsers", x => new { x.GroupId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_PermissionUsers_PermissionGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "PermissionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionUsers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"), "Operator" });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "AccountId", "AvailableSkillPoints", "AvailableStatusPoints", "BodyPart", "CreatedAt", "Dx", "Empire", "Experience", "GivenStatusPoints", "Gold", "HairPart", "Health", "Ht", "Iq", "Level", "Mana", "Name", "PlayTime", "PlayerClass", "PositionX", "PositionY", "SkillGroup", "St", "Stamina" },
                values: new object[] { 1u, new Guid("e34fd5ab-fb3b-428e-935b-7db5bd08a3e5"), 99u, 0u, 0u, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), (byte)99, (byte)EEmpire.Jinno, 0u, 0u, 2000000000u, 0u, 99999L, (byte)99, (byte)99, (byte)99, 99999L, "Admin", 0ul, (byte)0, 958870, 272788, (byte)0, (byte)99, 0L });

            migrationBuilder.InsertData(
                table: "PermissionUsers",
                columns: new[] { "GroupId", "PlayerId" },
                values: new object[] { new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"), 1u });

            migrationBuilder.CreateIndex(
                name: "IX_Items_PlayerId",
                table: "Items",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGroups_Name",
                table: "PermissionGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_GroupId",
                table: "Permissions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionUsers_PlayerId",
                table: "PermissionUsers",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeletedPlayers");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "PermissionUsers");

            migrationBuilder.DropTable(
                name: "PlayerSkills");

            migrationBuilder.DropTable(
                name: "PermissionGroups");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
