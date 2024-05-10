#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class AddAdminPlayerData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "players",
                columns: new[]
                {
                    "Id", "AccountId", "AvailableStatusPoints", "BodyPart", "CreatedAt", "Dx", "Empire", "Experience",
                    "GivenStatusPoints", "Gold", "HairPart", "Health", "Ht", "Iq", "Level", "Mana", "Name", "PlayTime",
                    "PlayerClass", "PositionX", "PositionY", "SkillGroup", "St", "Stamina"
                },
                values: new object[]
                {
                    new Guid("fefa4396-c5d1-4d7f-bc84-5df40867eac8"), new Guid("e34fd5ab-fb3b-428e-935b-7db5bd08a3e5"),
                    0u, 0u, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), (byte) 99, (byte) 0, 0u, 0u,
                    2000000000u, 0u, 99999L, (byte) 99, (byte) 99, (byte) 99, 99999L, "Admin", 0u, (byte) 0, 958870,
                    272788, (byte) 0, (byte) 99, 0L
                });

            migrationBuilder.InsertData(
                table: "perm_users",
                columns: new[] {"GroupId", "PlayerId"},
                values: new object[]
                {
                    new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"), new Guid("fefa4396-c5d1-4d7f-bc84-5df40867eac8")
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "perm_users",
                keyColumns: new[] {"GroupId", "PlayerId"},
                keyValues: new object[]
                {
                    new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"), new Guid("fefa4396-c5d1-4d7f-bc84-5df40867eac8")
                });

            migrationBuilder.DeleteData(
                table: "players",
                keyColumn: "Id",
                keyValue: new Guid("fefa4396-c5d1-4d7f-bc84-5df40867eac8"));
        }
    }
}