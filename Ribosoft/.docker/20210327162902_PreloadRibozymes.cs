using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ribosoft.Data.Migrations.NpgsqlMigrations
{
    public partial class PreloadRibozymes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var seedDateTime = new DateTime(2021, 3, 27, 16, 29, 2, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "Ribozymes",
                columns: new[] { "Id", "Name", "CreatedAt", "UpdatedAt"},
                values: new object[,] 
                {
                    { 1, "Default Pistol", seedDateTime, seedDateTime },
                    { 2, "Default Yarrowia", seedDateTime, seedDateTime },
                    { 3, "Default CRISPRi", seedDateTime, seedDateTime },
                    { 4, "Default FMN Aptazyme (Induced)", seedDateTime, seedDateTime },
                    { 5, "Default FMN Aptazyme (Inhibited)", seedDateTime, seedDateTime },
                    { 6, "Default Yarrowia Lipolytica", seedDateTime, seedDateTime },
                    { 7, "Default Twister", seedDateTime, seedDateTime },
                    { 8, "Default Twister Sister", seedDateTime, seedDateTime },
                    { 9, "Default Yarrowia - GUC", seedDateTime, seedDateTime },
                    { 10, "Default Pistol II", seedDateTime, seedDateTime },
                    { 11, "Default Extended Hammerhead", seedDateTime, seedDateTime },
                    { 12, "Default Theophylline Aptazyme", seedDateTime, seedDateTime }
                });

            migrationBuilder.InsertData(
                table: "RibozymeStructures",
                columns: new[] { "Id", "Cutsite", "PostProcess", "RibozymeId", "Sequence", "Structure", "SubstrateStructure", "SubstrateTemplate", "CreatedAt", "UpdatedAt" },
                values: new object[,] 
                {
                    { 1, 7, Convert.ToBoolean(0), 1, "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNNnnnnnn", "((((.[[[[[[.))))........0123.....]]]]]]...456789abcdef", "fedcba987654..3210", "nnnnnnNNNNNNGUNNNN", seedDateTime, seedDateTime },
                    { 2, 5, Convert.ToBoolean(0), 2, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNnnnnnnnnnnn", "01234567.......((........89abcdef..))...ghijklmnopqrstu", "utsrqponmlkjihg.76543210fedcba98", "nnnnnnnnnnnNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 3, 3, Convert.ToBoolean(0), 3, "NNNNNNNNNNNNNNNNNNNNGUUUUAGAGCUAGAAAUAGCAAGCUAAAACAAGGCUAGUCCGUUAUCAACUUGAAAAAGUGGCACCGAGUCGGUGCCUUUUUU", "0123456789abcdefghij(((((((.((((....))))...)))))))..............................(((((((...)))))))......", "...jihgfedcba9876543210", "CCNNNNNNNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 4, 12, Convert.ToBoolean(0), 4, "nnnnnNNNNNNCUGAUGAGCCUUAGGAUAUGCUUCGGCAGAAGGACGUCGAAACNNNNnnnnn", "0123456789a.......(.(.(......(((....))).....).).)...bcdefghijkl", "lkjihgfedcb.a9876543210", "nnnnnNNNNGUANNNNNNnnnnn", seedDateTime, seedDateTime },
                    { 5, 12, Convert.ToBoolean(0), 5, "nnnnnNNNNNNCUGAUGAGAUGAGGAUAUGCUUCGGCAGAAGGCUCUCGAAACNNNNnnnnn", "0123456789a.......(..(......(((....))).....)...)...bcdefghijkl", "lkjihgfedcb.a9876543210", "nnnnnNNNNGUANNNNNNnnnnn", seedDateTime, seedDateTime },
                    { 6, 5, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghij.......................", "jihg.76543210fedcba98", "NNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 7, 6, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijk.......................", "kjihg.76543210fedcba98", "NNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 8, 7, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijkl.......................", "lkjihg.76543210fedcba98", "NNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 9, 8, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklm.......................", "mlkjihg.76543210fedcba98", "NNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 10, 9, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmn.......................", "nmlkjihg.76543210fedcba98", "NNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 11, 10, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmno.......................", "onmlkjihg.76543210fedcba98", "NNNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 12, 11, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnop.......................", "ponmlkjihg.76543210fedcba98", "NNNNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 13, 12, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopq.......................", "qponmlkjihg.76543210fedcba98", "NNNNNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 14, 13, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqr.......................", "rqponmlkjihg.76543210fedcba98", "NNNNNNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 15, 14, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqrs.......................", "srqponmlkjihg.76543210fedcba98", "NNNNNNNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 16, 15, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqrst.......................", "tsrqponmlkjihg.76543210fedcba98", "NNNNNNNNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 17, 16, Convert.ToBoolean(0), 6, "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNNNNNNNNNNNNCUAUCUGCACUAGAUGCACCUUA", "01234567.......((........89abcdef..))...ghijklmnopqrstu.......................", "utsrqponmlkjihg.76543210fedcba98", "NNNNNNNNNNNNNGUCGNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 18, 5, Convert.ToBoolean(0), 7, "nnnnnnnnnnnnnnnNNNNCGGUYNCAAGCCCRNNNNGNRGAGNRNNRN", "0123456789abcdefgh(((..[[[..ij.)))..klmn..]]]opqr", "rqpo...nmlkjihgfedcba9876543210", "NNNNUAAYNCNGCNNNnnnnnnnnnnnnnnn", seedDateTime, seedDateTime },
                    { 19, 6, Convert.ToBoolean(0), 8, "NNNCGCAAGGCCNACGNNNNCGGCYNGUGCAAGCCNRGCNRYCNnnnnn", "0123.....456..(((..)))((((.........))))789abcdefg", "0123......456789abcdefg", "NNNGUACUCGCGGRYGGNnnnnn", seedDateTime, seedDateTime },
                    { 20, 7, Convert.ToBoolean(0), 8, "NNNCGCAAGGCCNACGNNNNCGGCYNGUGCAAGCCNRGCNRYCNnnnnn", "0123.....456..(((..)))((((.........))))789abcdefg", "0123.......456789abcdefg", "NNNGNUACUCGCGGRYGGNnnnnn", seedDateTime, seedDateTime },
                    { 21, 8, Convert.ToBoolean(0), 8, "NNNCGCAAGGCCNACGNNNNCGGCYNGUGCAAGCCNRGCNRYCNnnnnn", "0123.....456..(((..)))((((.........))))789abcdefg", "0123........456789abcdefg", "NNNGNNUACUCGCGGRYGGNnnnnn", seedDateTime, seedDateTime },
                    { 22, 16, Convert.ToBoolean(0), 9, "NNNNNNNNCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNnnnnnnnnnnn", "01234567.......((........89abcdef..))...ghijklmnopqrstu", "utsrqponmlkjihg.76543210fedcba98", "nnnnnnnnnnnNNGUCNNNNNNNNNNNNNNNN", seedDateTime, seedDateTime },
                    { 23, 11, Convert.ToBoolean(0), 10, "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNNNNNNnnnnnn", "((((.[[[[[[.))))........0123.....]]]]]]...456789abcdefghij", "jihgfedcba987654..3210", "nnnnnnNNNNNNNNNNGUNNNN", seedDateTime, seedDateTime },
                    { 24, 6, Convert.ToBoolean(0), 11, "nnnnnnnnNNAAUNNNNNCUGAUGAGUCGCUGAAAUGCGACGAAACNNNnnnnnnnnnn", "0123456789...abcde.......(((((......)))))...fghijklmnopqrst", "tsrqponmlkjihgf.edcba9876543210", "nnnnnnnnnnNNNGUCNNNNNNNnnnnnnnn", seedDateTime, seedDateTime },
                    { 25, 6, Convert.ToBoolean(0), 12, "nnnnnnnnnnnnNNNNNNCUGAUGAGCCUGGAUACCAGCCGAAAGGCCCUUGGCAGUUAGACGAAACNNNnnnnnnnnnn", "0123456789abcdefgh.......(.((((...((.(((....)))....))...)))).)...ijklmnopqrstuvw", "wvutsrqponmlkji.hgfedcba9876543210", "nnnnnnnnnnNNNGUCNNNNNNnnnnnnnnnnnn", seedDateTime, seedDateTime }
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