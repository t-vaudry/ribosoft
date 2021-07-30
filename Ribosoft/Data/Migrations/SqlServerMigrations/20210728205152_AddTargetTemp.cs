using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.SqlServerMigrations
{
    public partial class AddTargetTemp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
               name: "TargetTemperature",
               table: "Jobs",
               nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetTemperature",
                table: "Jobs");
        }
    }
}
