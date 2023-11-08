using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("empires")]
    [Migration(20231108233500)]
    public class CreateEmpiresTable : Migration
    {
        public override void Up()
        {
            Create.Table("empires")
                .WithColumn("Id").AsAnsiString(36).PrimaryKey()
                .WithColumn("AccountId").AsAnsiString(36).PrimaryKey()
                .WithColumn("Empire").AsByte().WithDefaultValue(0)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table("empires");
        }
    }
}
