using FluentMigrator;
using QuantumCore.Database;

namespace QuantumCore.Migrations
{
    [Tags("account")]
    [Migration(20201031160600)]
    public class CreateAccountTable : Migration
    {
        public override void Up()
        {
            Create.Table("accounts")
                .WithColumn("Id").AsAnsiString(36).PrimaryKey()
                .WithColumn("Username").AsString(30).NotNullable()
                .WithColumn("Password").AsString(60).NotNullable()
                .WithColumn("Email").AsString(100).NotNullable()
                .WithColumn("Status").AsInt16().NotNullable().WithDefaultValue(1)
                .WithColumn("LastLogin").AsDateTime().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table("accounts");
        }
    }
}