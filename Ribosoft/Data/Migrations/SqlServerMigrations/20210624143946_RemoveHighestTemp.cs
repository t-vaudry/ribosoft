using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.SqlServerMigrations
{
    public partial class RemoveHighestTemp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighestTemperatureScore",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "HighestTempTolerance",
                table: "Jobs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
               name: "HighestTemperatureScore",
               table: "Designs",
               nullable: true);

            migrationBuilder.AddColumn<float>(
               name: "HighestTempTolerance",
               table: "Jobs",
               nullable: true);
        }
    }
}


