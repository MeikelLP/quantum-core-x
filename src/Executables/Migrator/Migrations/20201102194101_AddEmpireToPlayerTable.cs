using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(20201102194101)]
    public class AddEmpireColumnToPlayersTable : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("players").Column("Empire").Exists())
            {
                Alter.Table("players")
                    .AddColumn("Empire").AsByte().WithDefaultValue(0);
            }
        }

        public override void Down()
        {
            Delete.Column("Empire").FromTable("players");
        }
    }
}
