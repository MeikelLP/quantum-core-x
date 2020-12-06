using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(202012052341)]
    public class CreateItemTable : Migration
    {
        public override void Up()
        {
            Create.Table("items")
                .WithColumn("Id").AsFixedLengthAnsiString(36).PrimaryKey()
                .WithColumn("PlayerId").AsFixedLengthAnsiString(36).ForeignKey("players", "id")
                .WithColumn("ItemId").AsInt32()
                .WithColumn("Window").AsByte()
                .WithColumn("Position").AsInt32()
                .WithColumn("Count").AsByte()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table("items");
        }
    }
}