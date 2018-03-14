using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.NpgsqlMigrations
{
    public partial class CutsiteIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CutsiteIndex",
                table: "Designs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubstrateSequenceLength",
                table: "Designs",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CutsiteIndex",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "SubstrateSequenceLength",
                table: "Designs");
        }
    }
}
