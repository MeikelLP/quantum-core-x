using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(20210808003600)]
    public class CreateDeletedPlayersTable : Migration
    {
        public override void Down()
        {
            Delete.Table("deleted_players");
        }

        public override void Up()
        {
            Create.Table("deleted_players")
                .WithColumn("Id").AsAnsiString(36).PrimaryKey()
                .WithColumn("AccountId").AsAnsiString(36).PrimaryKey()
                .WithColumn("PlayerClass").AsByte().NotNullable()
                .WithColumn("SkillGroup").AsByte().NotNullable()
                .WithColumn("PlayTime").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("Level").AsByte().NotNullable().WithDefaultValue(1)
                .WithColumn("Experience").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("Gold").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("St").AsByte().NotNullable().WithDefaultValue(0)
                .WithColumn("Ht").AsByte().NotNullable().WithDefaultValue(0)
                .WithColumn("Dx").AsByte().NotNullable().WithDefaultValue(0)
                .WithColumn("Iq").AsByte().NotNullable().WithDefaultValue(0)
                .WithColumn("PositionX").AsInt32().NotNullable()
                .WithColumn("PositionY").AsInt32().NotNullable()
                .WithColumn("Health").AsInt64().NotNullable()
                .WithColumn("Mana").AsInt64().NotNullable()
                .WithColumn("Stamina").AsInt64().NotNullable()
                .WithColumn("BodyPart").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("HairPart").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("DeletedAt").AsDateTime().NotNullable()
                .WithColumn("Name").AsAnsiString(24).NotNullable();
        }
    }
}
