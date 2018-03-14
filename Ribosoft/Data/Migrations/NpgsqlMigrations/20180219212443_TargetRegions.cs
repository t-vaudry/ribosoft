using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.NpgsqlMigrations
{
    public partial class TargetRegions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mg",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "Oligomer",
                table: "Jobs",
                newName: "Probe");

            migrationBuilder.RenameColumn(
                name: "TemperatureScore",
                table: "Designs",
                newName: "HighestTemperatureScore");

            migrationBuilder.AddColumn<bool>(
                name: "FivePrime",
                table: "Jobs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OpenReadingFrame",
                table: "Jobs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OpenReadingFrameEnd",
                table: "Jobs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpenReadingFrameStart",
                table: "Jobs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ThreePrime",
                table: "Jobs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "DesiredTemperatureScore",
                table: "Designs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FivePrime",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpenReadingFrame",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpenReadingFrameEnd",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OpenReadingFrameStart",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ThreePrime",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DesiredTemperatureScore",
                table: "Designs");

            migrationBuilder.RenameColumn(
                name: "Probe",
                table: "Jobs",
                newName: "Oligomer");

            migrationBuilder.RenameColumn(
                name: "HighestTemperatureScore",
                table: "Designs",
                newName: "TemperatureScore");

            migrationBuilder.AddColumn<float>(
                name: "Mg",
                table: "Jobs",
                nullable: true);
        }
    }
}
