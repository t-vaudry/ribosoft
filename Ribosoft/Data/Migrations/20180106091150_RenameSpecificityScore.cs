using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations
{
    public partial class RenameSpecificityScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpecializationScore",
                table: "Designs",
                newName: "SpecificityScore");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpecificityScore",
                table: "Designs",
                newName: "SpecializationScore");
        }
    }
}
