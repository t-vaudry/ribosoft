using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Ribosoft.Blast
{
    public class Blaster
    {
        public BlastParameters Parameters { get; set; }

        public Blaster()
        {
            Parameters = new BlastParameters();
        }

        public bool IsAvailable()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "blastn",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            string output;

            try
            {
                process.Start();

                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            }
            catch (Win32Exception)
            {
                // No such file
                return false;
            }
            finally
            {
                process.Close();
            }

            // primitive check that this is indeed blastn
            if (!output.StartsWith("blastn:", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public string Run()
        {
            return Run(this.Parameters);
        }

        public string Run(BlastParameters parameters)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "blastn",
                    Arguments = BuildArgumentString(parameters),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            if (!string.IsNullOrEmpty(parameters.BlastDbPath))
            {
                process.StartInfo.EnvironmentVariables["BLASTDB"] = parameters.BlastDbPath;
            }

            // start proc
            process.Start();

            // feed input
            process.StandardInput.Write(parameters.Query ?? "");
            process.StandardInput.Close();

            // read output
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // cleanup resources
            process.Close();

            return output;
        }

        public string BuildArgumentString(BlastParameters parameters)
        {
            var builder = new StringBuilder();

            // task
            string task;

            switch(parameters.Task)
            {
                case BlastParameters.BlastTask.blastn:
                    task = "blastn";
                    break;
                case BlastParameters.BlastTask.blastn_short:
                    task = "blastn-short";
                    break;
                case BlastParameters.BlastTask.dc_megablast:
                    task = "dc-megablast";
                    break;
                case BlastParameters.BlastTask.rmblastn:
                    task = "rmblastn";
                    break;
                case BlastParameters.BlastTask.megablast:
                default:
                    task = "megablast";
                    break;
            }

            builder.AppendFormat("-task {0} ", task);

            // db
            builder.AppendFormat("-db {0} ", EncodeParameterArgument(parameters.Database));

            // dust
            builder.AppendFormat("-dust {0} ", parameters.Dust ? "yes" : "no");

            // soft_masking
            builder.AppendFormat("-soft_masking {0} ", parameters.SoftMasking ? "true" : "false");

            // max_target_seqs
            builder.AppendFormat("-max_target_seqs {0} ", parameters.MaxTargetSequences);

            // use_index
            builder.AppendFormat("-use_index {0} ", parameters.UseIndex ? "true" : "false");

            // num_threads
            builder.AppendFormat("-num_threads {0} ", Math.Max(Math.Min(parameters.NumThreads, 8), 1));

            // outfmt
            builder.AppendFormat("-outfmt {0} ", EncodeParameterArgument(parameters.OutputFormat));

            // evalue
            builder.AppendFormat("-evalue {0} ", parameters.ExpectValue);

            return builder.ToString();
        }



        /// <summary>
        /// Encodes an argument for passing into a program
        /// </summary>
        /// <param name="original">The value that should be received by the program</param>
        /// <returns>The value which needs to be passed to the program for the original value 
        /// to come through</returns>
        public static string EncodeParameterArgument(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return original;
            }

            string value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
            value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"");

            return value;
        }
    }
}
