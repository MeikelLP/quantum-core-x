#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Auth.Persistence.Migrations.Mysql
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
                    name: "account_status",
                    columns: table => new
                    {
                        Id = table.Column<short>(type: "smallint", nullable: false)
                            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                        ClientStatus = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        AllowLogin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table => { table.PrimaryKey("PK_account_status", x => x.Id); })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                    name: "accounts",
                    columns: table => new
                    {
                        Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                        Username = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Password = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Status = table.Column<short>(type: "smallint", nullable: false),
                        LastLogin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                            defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                        DeleteCode = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
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
                    })
                .Annotation("MySql:CharSet", "utf8mb4");

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