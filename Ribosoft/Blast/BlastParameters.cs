using System;
namespace Ribosoft.Blast
{
    /*! \class BlastParameters
     * \brief Class used to define the parameters used for BLAST queries.
     */
    public class BlastParameters
    {
        /*! \enum BlastTask
         * \brief BLAST task from the command line tools
         */
        public enum BlastTask
        {
            blastn,
            blastn_short,
            dc_megablast,
            megablast,
            rmblastn
        }

        /*! \property Database
         * \brief BLAST database name
         */
        public string Database { get; set; } = "nt";
        
        /*! \property Query
         * \brief Query file name
         */
        public string Query { get; set; } = "";

        /*! \property Task
         * \brief BLAST task
         */
        public BlastTask Task { get; set; } = BlastTask.megablast;

        /*! \property Dust
         * \brief Filter query sequence with dust.
         */
        public bool Dust { get; set; } = true;

        /*! \property SoftMasking
         * \brief Apply filtering locations as soft masks (i.e., only for finding initial matches).
         */
        public bool SoftMasking { get; set; } = true;

        /*! \property MaxTargetSequences
         * \brief Number of aligned sequences to keep. Use with report formats that do not have separate definition line and alignment sections such as tabular (all outfmt > 4). Not compatible with num_descriptions or num_alignments. Ties are broken by order of sequences in the database.
         */
        public int MaxTargetSequences { get; set; } = 100;

        /*! \property UseIndex
         * \brief Use MegaBLAST database index. Indices may be created with the makembindex application.
         */
        public bool UseIndex { get; set; } = false;

        /*! \property NumThreads
         * \brief Number of threads (CPUs) to use in blast search.
         */
        public int NumThreads { get; set; } = 1;

        /*! \property OutputFormat
         * \brief alignment view options:
         * 0 = pairwise,
         * 1 = query-anchored showing identities,
         * 2 = query-anchored no identities,
         * 3 = flat query-anchored, show identities,
         * 4 = flat query-anchored, no identities,
         * 5 = XML Blast output,
         * 6 = tabular,
         * 7 = tabular with comment lines,
         * 8 = Text ASN.1,
         * 9 = Binary ASN.1
         * 10 = Comma-separated values
         * 11 = BLAST archive format (ASN.1)
         * 12 = Seqalign (JSON),
         * 13 = Multiple-file BLAST JSON,
         * 14 = Multiple-file BLAST XML2,
         * 15 = Single-file BLAST JSON,
         * 16 = Single-file BLAST XML2,
         * 17 = Sequence Alignment/Map (SAM),
         * 18 = Organism Report
         * Options 6, 7, and 10 can be additionally configured to produce a custom format specified by space delimited format specifiers.
         * The supported format specifiers are:
         * qseqid means Query Seq-id
         * qgi means Query GI
         * qacc means Query accesion
         * sseqid means Subject Seq-id
         * sallseqid means All subject Seq-id(s), separated by a ';'
         * sgi means Subject GI
         * sallgi means All subject GIs
         * sacc means Subject accession
         * sallacc means All subject accessions
         * qstart means Start of alignment in query
         * qend means End of alignment in query
         * sstart means Start of alignment in subject
         * send means End of alignment in subject
         * qseq means Aligned part of query sequence
         * sseq means Aligned part of subject sequence
         * evalue means Expect value
         * bitscore means Bit score
         * score means Raw score
         * length means Alignment length
         * pident means Percentage of identical matches
         * nident means Number of identical matches
         * mismatch means Number of mismatches
         * positive means Number of positive-scoring matches
         * gapopen means Number of gap openings
         * gaps means Total number of gap
         * ppos means Percentage of positive-scoring matches
         * frames means Query and subject frames separated by a '/'
         * qframe means Query frame
         * sframe means Subject frame
         * btop means Blast traceback operations (BTOP)
         * staxids means unique Subject Taxonomy ID(s), separated by a ';'(in numerical order)
         * sscinames means unique Subject Scientific Name(s), separated by a ';'
         * scomnames means unique Subject Common Name(s), separated by a ';'
         * sblastnames means unique Subject Blast Name(s), separated by a ';' (in alphabetical order)
         * sskingdoms means unique Subject Super Kingdom(s), separated by a ';' (in alphabetical order)
         * stitle means Subject Title
         * salltitles means All Subject Title(s), separated by a '<>'
         * sstrand means Subject Strand
         * qcovs means Query Coverage Per Subject (for all HSPs)
         * qcovhsp means Query Coverage Per HSP
         * qcovus is a measure of Query Coverage that counts a position in a subject sequence for this measure only once. The second time the position is aligned to the query is not counted towards this measure.
         * When not provided, the default value is:
         * 'qseqid sseqid pident length mismatch gapopen qstart qend sstart send evalue bitscore', which is equivalent to the keyword 'std'
         */
        public string OutputFormat { get; set; } = "6";

        /*! \property ExpectValue
         * \brief Expect value (E) for saving hits
         */
        public float ExpectValue { get; set; } = 10.0f;

        /*! \property BlastDbPath
         * \brief BLAST database path
         */
        public string BlastDbPath { get; set; } = "";

        /*! \property LowercaseMasking
         * \brief Use lower case filtering in query and subject sequence(s).
         */
        public bool LowercaseMasking { get; set; } = false;
    }
}
