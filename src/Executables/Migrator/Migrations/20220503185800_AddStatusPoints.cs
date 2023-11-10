using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(20220503185800)]
    public class AddStatusPoints : Migration
    {
        public override void Down()
        {
            Delete.Column("GivenStatusPoints").FromTable("players");
            Delete.Column("AvailableStatusPoints").FromTable("players");
        }

        public override void Up()
        {
            Alter.Table("players")
                .AddColumn("GivenStatusPoints").AsInt32().NotNullable().WithDefaultValue(0)
                .AddColumn("AvailableStatusPoints").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }
}
