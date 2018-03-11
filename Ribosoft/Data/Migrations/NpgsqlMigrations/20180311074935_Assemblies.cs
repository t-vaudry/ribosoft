using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.NpgsqlMigrations
{
    public partial class Assemblies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssemblyId",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecificityMethod",
                table: "Jobs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetEnvironment",
                table: "Jobs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Assemblies",
                columns: table => new
                {
                    TaxonomyId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AccessionId = table.Column<string>(nullable: false),
                    AssemblyName = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false),
                    OrganismName = table.Column<string>(nullable: false),
                    SpeciesId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assemblies", x => x.TaxonomyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_AssemblyId",
                table: "Jobs",
                column: "AssemblyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Assemblies_AssemblyId",
                table: "Jobs",
                column: "AssemblyId",
                principalTable: "Assemblies",
                principalColumn: "TaxonomyId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Assemblies_AssemblyId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "Assemblies");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_AssemblyId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AssemblyId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SpecificityMethod",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "TargetEnvironment",
                table: "Jobs");
        }
    }
}
