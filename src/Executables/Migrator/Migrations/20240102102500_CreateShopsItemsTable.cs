using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(20240102102500)]
    public class CreateShopItemsTable : Migration
    {
        public override void Up()
        {
            Create.Table("shop_items")
                .WithColumn("ShopId").AsAnsiString(36).ForeignKey()
                .WithColumn("ItemId").AsInt32().NotNullable()
                .WithColumn("Count").AsInt32().NotNullable().WithDefaultValue(1);
        }

        public override void Down()
        {
            Delete.Table("shop_items");
        }
    }
}