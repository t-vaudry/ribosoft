using System;
namespace Ribosoft.Blast
{
    public class BlastParameters
    {
        public enum BlastTask
        {
            blastn,
            blastn_short,
            dc_megablast,
            megablast,
            rmblastn
        }

        public string Database { get; set; } = "nt";
        public string Query { get; set; } = "";
        public BlastTask Task { get; set; } = BlastTask.megablast;
        public bool Dust { get; set; } = true;
        public bool SoftMasking { get; set; } = true;
        public int MaxTargetSequences { get; set; } = 100;
        public bool UseIndex { get; set; } = false;
        public int NumThreads { get; set; } = 1;
        public string OutputFormat { get; set; } = "6";
        public float ExpectValue { get; set; } = 10.0f;
        public string BlastDbPath { get; set; } = "";
        public bool LowercaseMasking { get; set; } = false;
    }
}
