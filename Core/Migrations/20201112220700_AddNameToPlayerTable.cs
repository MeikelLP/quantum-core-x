using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Migration(20201112220700)]
    [Tags("game")]
    public class AddNameToPlayerTable : Migration
    {
        public override void Up()
        {
            Alter.Table("players").AddColumn("Name").AsAnsiString(24).NotNullable().WithDefaultValue("");
            Alter.Table("players").AlterColumn("Name").AsAnsiString(24).NotNullable();
        }

        public override void Down()
        {
            Delete.Column("Name").FromTable("players");
        }
    }
}