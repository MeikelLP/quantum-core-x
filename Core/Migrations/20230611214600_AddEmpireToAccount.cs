using FluentMigrator;

namespace QuantumCore.Migrations;

[Tags("account")]
[Migration(20230611214600)]
public class AddEmpireToAccount : Migration{
    
    public override void Down()
    {
        Delete.Column("Empire").FromTable("accounts");
    }
    
    public override void Up()
    {
        Alter.Table("accounts")
            .AddColumn("Empire").AsByte().NotNullable().WithDefaultValue(0);
    }
}