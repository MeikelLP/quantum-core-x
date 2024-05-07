#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

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
                    name: "deleted_players",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        AccountId =
                            table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                        Name = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table => { table.PrimaryKey("PK_deleted_players", x => x.Id); })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "perm_groups",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(uuid())",
                            collation: "ascii_general_ci"),
                        Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table => { table.PrimaryKey("PK_perm_groups", x => x.Id); })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "players",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(uuid())",
                            collation: "ascii_general_ci"),
                        AccountId =
                            table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        Empire = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        PlayerClass = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        SkillGroup = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        PlayTime = table.Column<uint>(type: "int unsigned", nullable: false),
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
                        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        Name = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        GivenStatusPoints = table.Column<uint>(type: "int unsigned", nullable: false),
                        AvailableStatusPoints = table.Column<uint>(type: "int unsigned", nullable: false)
                    },
                    constraints: table => { table.PrimaryKey("PK_players", x => x.Id); })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "perm_auth",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(uuid())",
                            collation: "ascii_general_ci"),
                        GroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        Command = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_perm_auth", x => x.Id);
                        table.ForeignKey(
                            name: "FK_perm_auth_perm_groups_GroupId",
                            column: x => x.GroupId,
                            principalTable: "perm_groups",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "items",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(uuid())",
                            collation: "ascii_general_ci"),
                        PlayerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        ItemId = table.Column<uint>(type: "int unsigned", nullable: false),
                        Window = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        Position = table.Column<uint>(type: "int unsigned", nullable: false),
                        Count = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_items", x => x.Id);
                        table.ForeignKey(
                            name: "FK_items_players_PlayerId",
                            column: x => x.PlayerId,
                            principalTable: "players",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "perm_users",
                    columns: table => new
                    {
                        GroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        PlayerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_perm_users", x => new {x.GroupId, x.PlayerId});
                        table.ForeignKey(
                            name: "FK_perm_users_perm_groups_GroupId",
                            column: x => x.GroupId,
                            principalTable: "perm_groups",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_perm_users_players_PlayerId",
                            column: x => x.PlayerId,
                            principalTable: "players",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_items_PlayerId",
                table: "items",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_perm_auth_GroupId",
                table: "perm_auth",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_perm_groups_Name",
                table: "perm_groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_perm_users_PlayerId",
                table: "perm_users",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deleted_players");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "perm_auth");

            migrationBuilder.DropTable(
                name: "perm_users");

            migrationBuilder.DropTable(
                name: "perm_groups");

            migrationBuilder.DropTable(
                name: "players");
        }
    }
}