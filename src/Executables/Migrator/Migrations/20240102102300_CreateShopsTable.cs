using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(20240102102300)]
    public class CreateShopsTable : Migration
    {
        public override void Up()
        {
            Create.Table("shops")
                .WithColumn("Id").AsAnsiString(36).PrimaryKey()
                .WithColumn("Vnum").AsInt32().NotNullable().ForeignKey()
                .WithColumn("Name").AsAnsiString(24).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("shops");
        }
    }
}