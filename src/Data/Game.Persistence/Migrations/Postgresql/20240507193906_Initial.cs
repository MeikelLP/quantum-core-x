#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Postgresql
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerClass = table.Column<byte>(type: "smallint", nullable: false),
                    SkillGroup = table.Column<byte>(type: "smallint", nullable: false),
                    PlayTime = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<byte>(type: "smallint", nullable: false),
                    St = table.Column<byte>(type: "smallint", nullable: false),
                    Ht = table.Column<byte>(type: "smallint", nullable: false),
                    Dx = table.Column<byte>(type: "smallint", nullable: false),
                    Iq = table.Column<byte>(type: "smallint", nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<long>(type: "bigint", nullable: false),
                    Mana = table.Column<long>(type: "bigint", nullable: false),
                    Stamina = table.Column<long>(type: "bigint", nullable: false),
                    BodyPart = table.Column<int>(type: "integer", nullable: false),
                    HairPart = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_deleted_players", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "perm_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_perm_groups", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Empire = table.Column<byte>(type: "smallint", nullable: false),
                    PlayerClass = table.Column<byte>(type: "smallint", nullable: false),
                    SkillGroup = table.Column<byte>(type: "smallint", nullable: false),
                    PlayTime = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false),
                    St = table.Column<byte>(type: "smallint", nullable: false),
                    Ht = table.Column<byte>(type: "smallint", nullable: false),
                    Dx = table.Column<byte>(type: "smallint", nullable: false),
                    Iq = table.Column<byte>(type: "smallint", nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<long>(type: "bigint", nullable: false),
                    Mana = table.Column<long>(type: "bigint", nullable: false),
                    Stamina = table.Column<long>(type: "bigint", nullable: false),
                    BodyPart = table.Column<long>(type: "bigint", nullable: false),
                    HairPart = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    Name = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    GivenStatusPoints = table.Column<long>(type: "bigint", nullable: false),
                    AvailableStatusPoints = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_players", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "perm_auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Command = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Window = table.Column<byte>(type: "smallint", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<byte>(type: "smallint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
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
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false)
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