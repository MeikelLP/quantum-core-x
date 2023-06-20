using FluentMigrator;
using QuantumCore.Database;

namespace QuantumCore.Migrations
{
    [Tags("account")]
    [Migration(20201031172700)]
    public class CreateAccountStatus : Migration
    {
        public override void Up()
        {
            Create.Table("account_status")
                .WithColumn("Id").AsInt16().PrimaryKey().Identity()
                .WithColumn("ClientStatus").AsAnsiString(8).NotNullable()
                .WithColumn("AllowLogin").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("Description").AsString().Nullable();
            
            Update.Table("accounts").Set(new { Status = 1 }).Where(new { Status = 0 });

            Insert.IntoTable("account_status").Row(new AccountStatus
            {
                Id = 1,
                AllowLogin = true,
                ClientStatus = "OK"
            });

            Create.ForeignKey("fk_accounts_status").FromTable("accounts").ForeignColumn("Status")
                .ToTable("account_status").PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_accounts_status").OnTable("accounts");
            Delete.Table("account_status");
        }
    }
}