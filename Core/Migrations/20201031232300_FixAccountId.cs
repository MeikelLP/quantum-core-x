using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("account")]
    [Migration(20201031232300)]
    public class FixAccountId : Migration
    {
        public override void Up()
        {
            Alter.Table("accounts").AlterColumn("Id").AsFixedLengthAnsiString(36);
        }

        public override void Down()
        {
            Alter.Table("accounts").AlterColumn("Id").AsAnsiString(36);
        }
    }
}