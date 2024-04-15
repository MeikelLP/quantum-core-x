using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using QuantumCore.Database;

namespace QuantumCore.Migrations
{
    [Tags("account")]
    [Migration(20231101165500)]
    public class AlterAccountAddEmpire : Migration
    {
        public override void Down()
        {
            Delete.Column("Empire").FromTable("accounts");
        }

        public override void Up()
        {
            Alter.Table("accounts").AddColumn("Empire").AsByte().WithDefaultValue(0);
        }
    }
}
