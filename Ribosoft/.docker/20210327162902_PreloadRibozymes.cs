using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.NpgsqlMigrations
{
    public partial class PreloadRibozymes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Ribozymes",
                columns: new[] { "Id", "Name"},
                values: new object[,] 
                {
                    { 1, "Default Pistol" },
                    { 2, "Default Yarrowia" },
                    { 3, "Default CRISPRi" },
                    { 4, "Default FMN Aptazyme (Induced)" },
                    { 5, "Default FMN Aptazyme (Inhibited)" },
                    { 6, "Default Yarrowia Lipolytica" },
                    { 7, "Default Twister" },
                    { 8, "Default Twister Sister" },
                    { 9, "Default Yarrowia - GUC" },
                    { 10, "Default Pistol II" },
                    { 11, "Default Extended Hammerhead" },
                    { 12, "Default Theophylline Aptazyme" }
                });

            migrationBuilder.InsertData(
                table: "RibozymeStructures",
                columns: new[] { "Id", "Cutsite", "PostProcess", "RibozymeId", "Sequence", "Structure", "SubstrateStructure", "SubstrateTemplate" },
                values: new object[,] 
                {
                    { 1, 7, Convert.ToBoolean(0), 1, "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNNnnnnnn", "((((.[[[[[[.))))........0123.....]]]]]]...456789abcdef", "fedcba987654..3210", "nnnnnnNNNNNNGUNNNN" },
                    { 2, 5, Convert.ToBoolean(0), 2, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNnnnnnnnnnnn", "01234567.......((........89abcdef..))...ghijklmnopqrstu", "utsrqponmlkjihg.76543210fedcba98", "nnnnnnnnnnnNNGUCGNNNNNNNNNNNNNNN" },
                    { 3, 3, Convert.ToBoolean(0), 3, "NNNNNNNNNNNNNNNNNNNNGUUUUAGAGCUAGAAAUAGCAAGCUAAAACAAGGCUAGUCCGUUAUCAACUUGAAAAAGUGGCACCGAGUCGGUGCCUUUUUU", "0123456789abcdefghij(((((((.((((....))))...)))))))..............................(((((((...)))))))......", "...jihgfedcba9876543210", "CCNNNNNNNNNNNNNNNNNNNNN" },
                    { 4, 12, Convert.ToBoolean(0), 4, "nnnnnNNNNNNCUGAUGAGCCUUAGGAUAUGCUUCGGCAGAAGGACGUCGAAACNNNNnnnnn", "0123456789a.......(.(.(......(((....))).....).).)...bcdefghijkl", "lkjihgfedcb.a9876543210", "nnnnnNNNNGUANNNNNNnnnnn" },
                    { 5, 12, Convert.ToBoolean(0), 5, "nnnnnNNNNNNCUGAUGAGAUGAGGAUAUGCUUCGGCAGAAGGCUCUCGAAACNNNNnnnnn", "0123456789a.......(..(......(((....))).....)...)...bcdefghijkl", "lkjihgfedcb.a9876543210", "nnnnnNNNNGUANNNNNNnnnnn" },
                    { 6, 5, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghij.......................", "jihg.76543210fedcba98", "NNGUCGNNNNNNNNNNNNNNN" },
                    { 7, 6, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijk.......................", "kjihg.76543210fedcba98", "NNNGUCGNNNNNNNNNNNNNNN" },
                    { 8, 7, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijkl.......................", "lkjihg.76543210fedcba98", "NNNNGUCGNNNNNNNNNNNNNNN" },
                    { 9, 8, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklm.......................", "mlkjihg.76543210fedcba98", "NNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 10, 9, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmn.......................", "nmlkjihg.76543210fedcba98", "NNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 11, 10, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmno.......................", "onmlkjihg.76543210fedcba98", "NNNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 12, 11, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnop.......................", "ponmlkjihg.76543210fedcba98", "NNNNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 13, 12, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopq.......................", "qponmlkjihg.76543210fedcba98", "NNNNNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 14, 13, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqr.......................", "rqponmlkjihg.76543210fedcba98", "NNNNNNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 15, 14, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqrs.......................", "srqponmlkjihg.76543210fedcba98", "NNNNNNNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 16, 15, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqrst.......................", "tsrqponmlkjihg.76543210fedcba98", "NNNNNNNNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 17, 16, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqrstu.......................", "utsrqponmlkjihg.76543210fedcba98", "NNNNNNNNNNNNNGUCGNNNNNNNNNNNNNNN" },
                    { 18, 5, Convert.ToBoolean(0), 7, "nnnnnnnnnnnnnnnNNNNCGGUYNCAAGCCCRNNNNGNRGAGNRNNRN", "0123456789abcdefgh(((..[[[..ij.)))..klmn..]]]opqr", "rqpo...nmlkjihgfedcba9876543210", "NNNNUAAYNCNGCNNNnnnnnnnnnnnnnnn" },
                    { 19, 6, Convert.ToBoolean(0), 8, "NNNCGCAAGGCCNACGNNNNCGGCYNGUGCAAGCCNRGCNRYCNnnnnn", "0123.....456..(((..)))((((.........))))789abcdefg", "0123......456789abcdefg", "NNNGUACUCGCGGRYGGNnnnnn" },
                    { 20, 7, Convert.ToBoolean(0), 8, "NNNCGCAAGGCCNACGNNNNCGGCYNGUGCAAGCCNRGCNRYCNnnnnn", "0123.....456..(((..)))((((.........))))789abcdefg", "0123.......456789abcdefg", "NNNGNUACUCGCGGRYGGNnnnnn" },
                    { 21, 8, Convert.ToBoolean(0), 8, "NNNCGCAAGGCCNACGNNNNCGGCYNGUGCAAGCCNRGCNRYCNnnnnn", "0123.....456..(((..)))((((.........))))789abcdefg", "0123........456789abcdefg", "NNNGNNUACUCGCGGRYGGNnnnnn" },
                    { 22, 16, Convert.ToBoolean(0), 9, "NNNNNNNNCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNnnnnnnnnnnn", "01234567.......((........89abcdef..))...ghijklmnopqrstu", "utsrqponmlkjihg.76543210fedcba98", "nnnnnnnnnnnNNGUCNNNNNNNNNNNNNNNN" },
                    { 23, 11, Convert.ToBoolean(0), 10, "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNNNNNNnnnnnn", "((((.[[[[[[.))))........0123.....]]]]]]...456789abcdefghij", "jihgfedcba987654..3210", "nnnnnnNNNNNNNNNNGUNNNN" },
                    { 24, 6, Convert.ToBoolean(0), 11, "nnnnnnnnNNAAUNNNNNCUGAUGAGUCGCUGAAAUGCGACGAAACNNNnnnnnnnnnn", "0123456789...abcde.......(((((......)))))...fghijklmnopqrst", "tsrqponmlkjihgf.edcba9876543210", "nnnnnnnnnnNNNGUCNNNNNNNnnnnnnnn" },
                    { 25, 6, Convert.ToBoolean(0), 12, "nnnnnnnnnnnnNNNNNNCUGAUGAGCCUGGAUACCAGCCGAAAGGCCCUUGGCAGUUAGACGAAACNNNnnnnnnnnnn", "0123456789abcdefgh.......(.((((...((.(((....)))....))...)))).)...ijklmnopqrstuvw", "wvutsrqponmlkji.hgfedcba9876543210", "nnnnnnnnnnNNNGUCNNNNNNnnnnnnnnnnnn" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 1);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 2);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 3);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 4);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 5);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 6);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 7);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 8);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 9);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 10);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 11);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 12);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 13);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 14);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 15);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 16);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 17);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 18);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 19);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 20);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 21);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 22);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 23);

            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 24);
            
            migrationBuilder.DeleteData(
            table: "RibozymeStructures",
            keyColumn: "Id",
            keyValue: 25);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 1);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 2);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 3);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 4);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 5);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 6);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 7);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 8);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 9);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 10);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 11);

            migrationBuilder.DeleteData(
            table: "Ribozymes",
            keyColumn: "Id",
            keyValue: 12);
        }
    }
}