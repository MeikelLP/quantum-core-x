using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuantumCore.Game.Persistence.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeletedPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlayerClass = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillGroup = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PlayTime = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    St = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Ht = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Dx = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Iq = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    Health = table.Column<long>(type: "bigint", nullable: false),
                    Mana = table.Column<long>(type: "bigint", nullable: false),
                    Stamina = table.Column<long>(type: "bigint", nullable: false),
                    BodyPart = table.Column<int>(type: "int", nullable: false),
                    HairPart = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedPlayers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionGroups", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                    Empire = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PlayerClass = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillGroup = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PlayTime = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Experience = table.Column<uint>(type: "int unsigned", nullable: false),
                    Gold = table.Column<uint>(type: "int unsigned", nullable: false),
                    St = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Ht = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Dx = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Iq = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    Health = table.Column<long>(type: "bigint", nullable: false),
                    Mana = table.Column<long>(type: "bigint", nullable: false),
                    Stamina = table.Column<long>(type: "bigint", nullable: false),
                    BodyPart = table.Column<uint>(type: "int unsigned", nullable: false),
                    HairPart = table.Column<uint>(type: "int unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GivenStatusPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    AvailableStatusPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                    AvailableSkillPoints = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlayerSkills",
                columns: table => new
                {
                    PlayerId = table.Column<uint>(type: "int unsigned", nullable: false),
                    SkillId = table.Column<uint>(type: "int unsigned", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                    MasterType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NextReadTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSkills", x => new { x.PlayerId, x.SkillId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SkillProtos",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    LevelStep = table.Column<short>(type: "smallint", nullable: false),
                    MaxLevel = table.Column<short>(type: "smallint", nullable: false),
                    LevelLimit = table.Column<short>(type: "smallint", nullable: false),
                    PointOn = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SPCostPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationSPCostPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CooldownPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MasterBonusPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttackGradePoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Flags = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AffectFlags = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointOn2 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointPoly2 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationPoly2 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AffectFlags2 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointOn3 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointPoly3 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationPoly3 = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GrandMasterAddSPCostPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrerequisiteSkillVnum = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteSkillLevel = table.Column<int>(type: "int", nullable: false),
                    SkillType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxHit = table.Column<short>(type: "smallint", nullable: false),
                    SplashAroundDamageAdjustPoly = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetRange = table.Column<int>(type: "int", nullable: false),
                    SplashRange = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillProtos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Command = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlayerId = table.Column<uint>(type: "int unsigned", nullable: false),
                    ItemId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Window = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Position = table.Column<uint>(type: "int unsigned", nullable: false),
                    Count = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionUsers",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlayerId = table.Column<uint>(type: "int unsigned", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"), "Operator" });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "AccountId", "AvailableSkillPoints", "AvailableStatusPoints", "BodyPart", "CreatedAt", "Dx", "Empire", "Experience", "GivenStatusPoints", "Gold", "HairPart", "Health", "Ht", "Iq", "Level", "Mana", "Name", "PlayTime", "PlayerClass", "PositionX", "PositionY", "SkillGroup", "St", "Stamina" },
                values: new object[] { 1u, new Guid("e34fd5ab-fb3b-428e-935b-7db5bd08a3e5"), 99u, 0u, 0u, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), (byte)99, (byte)0, 0u, 0u, 2000000000u, 0u, 99999L, (byte)99, (byte)99, (byte)99, 99999L, "Admin", 0ul, (byte)0, 958870, 272788, (byte)0, (byte)99, 0L });

            migrationBuilder.InsertData(
                table: "SkillProtos",
                columns: new[] { "Id", "AffectFlags", "AffectFlags2", "AttackGradePoly", "CooldownPoly", "DurationPoly", "DurationPoly2", "DurationPoly3", "DurationSPCostPoly", "Flags", "GrandMasterAddSPCostPoly", "LevelLimit", "LevelStep", "MasterBonusPoly", "MaxHit", "MaxLevel", "Name", "PointOn", "PointOn2", "PointOn3", "PointPoly", "PointPoly2", "PointPoly3", "PrerequisiteSkillLevel", "PrerequisiteSkillVnum", "SPCostPoly", "SkillType", "SplashAroundDamageAdjustPoly", "SplashRange", "TargetRange", "Type" },
                values: new object[,]
                {
                    { 1u, "Ymir", "Ymir", "", "12", "", "", "", "", "Attack,UseMeleeDamage", "40+100*k", (short)0, (short)1, "-( 1.1*atk + (0.5*atk +  1.5 * str)*k)", (short)5, (short)1, "»ï¿¬Âü", "HP", "None", "", "-( 1.1*atk + (0.5*atk +  1.5 * str)*k)", "", "", 0, 0, "40+100*k", "Melee", "1", 0u, 0, (short)1 },
                    { 2u, "Ymir", "Ymir", "", "15", "", "", "", "", "Attack,UseMeleeDamage", "50+130*k", (short)0, (short)1, "-(3*atk + (0.8*atk + str*5 + dex*3 +con)*k)", (short)12, (short)1, "ÆÈ¹æÇ³¿ì", "HP", "None", "", "-(3*atk + (0.8*atk + str*5 + dex*3 +con)*k)", "", "", 0, 0, "50+130*k", "Melee", "1", 200u, 0, (short)1 },
                    { 3u, "Jeongwihon", "Ymir", "", "63+10*k", "60+90*k", "60+90*k", "", "", "SelfOnly", "50+140*k", (short)0, (short)1, "50*k", (short)1, (short)1, "Àü±ÍÈ¥", "ATT_SPEED", "MOV_SPEED", "", "50*k", "20*k", "", 0, 0, "50+140*k", "Normal", "1", 0u, 0, (short)1 },
                    { 4u, "Geomgyeong", "Ymir", "", "30+10*k", "30+50*k", "", "", "", "SelfOnly", "100+200*k", (short)0, (short)1, "(100 + str + lv * 3)*k", (short)1, (short)1, "°Ë°æ", "ATT_GRADE", "NONE", "", "(100 + str + lv * 3)*k", "", "", 0, 0, "100+200*k", "Normal", "1", 0u, 0, (short)1 },
                    { 5u, "Ymir", "Ymir", "", "12", "", "3", "", "", "Attack,UseMeleeDamage,Splash,Crush", "60+120*k", (short)0, (short)1, "-(2*atk + (atk + dex*3 + str*7 + con)*k)", (short)4, (short)1, "ÅºÈ¯°Ý", "HP", "MOV_SPEED", "", "-(2*atk + (atk + dex*3 + str*7 + con)*k)", "150", "", 0, 0, "60+120*k", "Melee", "1", 200u, 0, (short)1 }
                });

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
                name: "SkillProtos");

            migrationBuilder.DropTable(
                name: "PermissionGroups");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
