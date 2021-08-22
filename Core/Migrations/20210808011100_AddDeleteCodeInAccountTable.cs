using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("account")]
    [Migration(20210808011100)]
    public class AddDeleteCodeInAccountTable : Migration
    {
        public override void Down()
        {
            Delete.Column("DeleteCode").FromTable("accounts");
        }

        public override void Up()
        {
            Alter.Table("accounts").AddColumn("DeleteCode").AsAnsiString(7).NotNullable().WithDefaultValue("1234567");
            Alter.Table("accounts").AlterColumn("DeleteCode").AsAnsiString(7).NotNullable().WithDefaultValue("1234567");
        }
    }
}
