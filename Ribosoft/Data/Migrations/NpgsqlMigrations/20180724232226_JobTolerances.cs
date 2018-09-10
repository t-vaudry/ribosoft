using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.NpgsqlMigrations
{
    public partial class JobTolerances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Assemblies_AssemblyId",
                table: "Jobs");

            migrationBuilder.AlterColumn<string>(
                name: "SubstrateTemplate",
                table: "RibozymeStructures",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubstrateStructure",
                table: "RibozymeStructures",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Structure",
                table: "RibozymeStructures",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Sequence",
                table: "RibozymeStructures",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ribozymes",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "AccessibilityTolerance",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "DesiredTempTolerance",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "HighestTempTolerance",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SpecificityTolerance",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "StructureTolerance",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Assemblies_AssemblyId",
                table: "Jobs",
                column: "AssemblyId",
                principalTable: "Assemblies",
                principalColumn: "TaxonomyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Assemblies_AssemblyId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AccessibilityTolerance",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DesiredTempTolerance",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "HighestTempTolerance",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SpecificityTolerance",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StructureTolerance",
                table: "Jobs");

            migrationBuilder.AlterColumn<string>(
                name: "SubstrateTemplate",
                table: "RibozymeStructures",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "SubstrateStructure",
                table: "RibozymeStructures",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Structure",
                table: "RibozymeStructures",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Sequence",
                table: "RibozymeStructures",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ribozymes",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Assemblies_AssemblyId",
                table: "Jobs",
                column: "AssemblyId",
                principalTable: "Assemblies",
                principalColumn: "TaxonomyId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
