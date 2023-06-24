using FluentMigrator;
using QuantumCore.Database;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(20230624213700)]
    public class CreateAffectsTable : Migration
    {
        public override void Up()
        {
            Create.Table("affects")
                .WithColumn("PlayerId").AsGuid().PrimaryKey()
                .WithColumn("Type").AsInt64().PrimaryKey()
                .WithColumn("ApplyOn").AsByte().PrimaryKey()
                .WithColumn("ApplyValue").AsInt32().PrimaryKey()
                .WithColumn("Flag").AsInt32().NotNullable()
                .WithColumn("Duration").AsDateTime().NotNullable()
                .WithColumn("SpCost").AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("affects");
        }
    }
}