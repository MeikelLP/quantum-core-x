using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Migration(20201113191300)]
    [Tags("game")]
    public class FixPlayerIds : Migration
    {
        public override void Up()
        {
            Alter.Table("players").AlterColumn("Id").AsFixedLengthAnsiString(36);
            Alter.Table("players").AlterColumn("AccountId").AsFixedLengthAnsiString(36);
        }

        public override void Down()
        {
            Alter.Table("players").AlterColumn("Id").AsAnsiString(36);
            Alter.Table("players").AlterColumn("AccountId").AsAnsiString(36);
        }
    }
}