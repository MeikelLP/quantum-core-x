#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Auth.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_status",
                columns: table => new
                {
                    Id = table.Column<short>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientStatus = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    AllowLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_account_status", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<short>(type: "INTEGER", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false,
                        defaultValueSql: "current_timestamp"),
                    DeleteCode = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_account_status_Status",
                        column: x => x.Status,
                        principalTable: "account_status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "account_status",
                columns: new[] {"Id", "AllowLogin", "ClientStatus", "Description"},
                values: new object[] {(short) 1, true, "OK", "Default Status"});

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] {"Id", "DeleteCode", "Email", "LastLogin", "Password", "Status", "Username"},
                values: new object[]
                {
                    new Guid("e34fd5ab-fb3b-428e-935b-7db5bd08a3e5"), "1234567", "admin@test.com", null,
                    "$2y$10$5e9nP50E64iy8vaSMwrRWO7vCfnA7.p5XpIDHC3hPdi6BCtTF7rBS", (short) 1, "admin"
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_Status",
                table: "accounts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "account_status");
        }
    }
}