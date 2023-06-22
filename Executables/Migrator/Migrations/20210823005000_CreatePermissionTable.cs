using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using QuantumCore.Database;

namespace QuantumCore.Migrations
{
    [Tags("game")]
    [Migration(20210823005000)]
    public class CreatePermissionTable : Migration
    {
        public override void Down()
        {
            Delete.Table("perm_auth");
            Delete.Table("perm_groups");
            Delete.Table("perm_users");
        }

        public override void Up()
        {
            Create.Table("perm_auth")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Group").AsAnsiString(36).NotNullable()
                .WithColumn("Command").AsAnsiString().NotNullable();

            Create.Table("perm_groups")
                .WithColumn("Id").AsAnsiString(36).PrimaryKey()
                .WithColumn("Name").AsString(30).NotNullable();

            Create.Table("perm_users")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Group").AsAnsiString(36).NotNullable()
                .WithColumn("Player").AsAnsiString(36).NotNullable();
        }
    }
}
