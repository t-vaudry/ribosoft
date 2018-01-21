using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.NpgsqlMigrations
{
    public partial class AddJobParams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Mg",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Na",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Oligomer",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Temperature",
                table: "Jobs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mg",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Na",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Oligomer",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "Jobs");
        }
    }
}
