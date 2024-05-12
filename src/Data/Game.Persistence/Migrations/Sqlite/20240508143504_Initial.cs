#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deleted_players",
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
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_deleted_players", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "perm_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_perm_groups", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Empire = table.Column<byte>(type: "INTEGER", nullable: false),
                    PlayerClass = table.Column<byte>(type: "INTEGER", nullable: false),
                    SkillGroup = table.Column<byte>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<uint>(type: "INTEGER", nullable: false),
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
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    GivenStatusPoints = table.Column<uint>(type: "INTEGER", nullable: false),
                    AvailableStatusPoints = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_players", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "perm_auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Command = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Window = table.Column<byte>(type: "INTEGER", nullable: false),
                    Position = table.Column<uint>(type: "INTEGER", nullable: false),
                    Count = table.Column<byte>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp")
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
                });

            migrationBuilder.CreateTable(
                name: "perm_users",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false)
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
                });

            migrationBuilder.InsertData(
                table: "perm_groups",
                columns: new[] {"Id", "Name"},
                values: new object[] {new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"), "Operator"});

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