using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Ribosoft.Models;
using System.Linq;

namespace Ribosoft
{
    /*! \struct FoldOutput
     * \brief Structure used for the output of the folding algorithm
     */
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct FoldOutput
    {
        /*! \property Structure
         * \brief Fold output structure
         */
        public string Structure;

        /*! \property Probability
         * \brief Fold output probability
         */
        public float Probability;
    }

    /*! \class RibosoftAlgo
     * \brief Wrapper class to import dll functionality from RibosoftAlgo nuget package
     */
    public class RibosoftAlgo
    {
        /*! \fn validate_sequence
         * \brief DllImport from RibosoftAlgo of validate_sequence
         * \param sequence Sequence being validated
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS validate_sequence(string sequence);

        /*! \fn validate_structure
         * \brief DllImport from RibosoftAlgo of validate_structure
         * \param structure Structure being validated
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS validate_structure(string structure);

        /*! \fn accessibility
         * \brief DllImport from RibosoftAlgo of accessibility
         * \param substrateSequence Sequence of the substrate
         * \param substrateStructure Structure of the substrate
         * \param foldedStructure Structure of the folded RNA
         * \param na_concentration Concentration of sodium
         * \param probe_concentration Concentration of probe
         * \param delta Out parameter for the evaluation score
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS accessibility(string substrateSequence, string substrateStructure, string foldedStructure, float na_concentration, float probe_concentration, float targetTemperature, out float score);

        /*! \fn anneal
         * \brief DllImport from RibosoftAlgo of anneal
         * \param sequence RNA sequence
         * \param structure RNA structure
         * \param na_concentration Concentration of sodium
         * \param probe_concentration Concentration of probe
         * \param target_temp Target temperature of binding arms
         * \param temp Out parameter for the evaluation score
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS anneal(string sequence, string structure, float na_concentration, float probe_concentration, float target_temp, out float temp);

        /*! \fn fold
         * \brief DllImport from RibosoftAlgo of fold
         * \param sequence RNA sequence
         * \param output Output pointer to the list of fold outputs
         * \param size Out value of the size of the list
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS fold(string sequence, float env_temp, out IntPtr output, out int size);

        /*! \fn fold_output_free
         * \brief DllImport from RibosoftAlgo of fold_output_free
         * \param output Pointer to fold output list
         * \param size Size of the list
         */
        [DllImport("RibosoftAlgo")]
        private static extern void fold_output_free(IntPtr output, int size);

        /*! \fn default_fold
         * \brief DllImport from RibosoftAlgo of mfe_default_fold
         * \param sequence Sequence to be folded
         * \param structure Output pointer to the folded structure
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS mfe_default_fold(string sequence, float env_temp, out IntPtr structure);

        /*! \fn fold_output_free
         * \brief DllImport from RibosoftAlgo of mfe_default_fold_free
         * \param output Pointer to fold output list
         */
        [DllImport("RibosoftAlgo")]
        private static extern void mfe_default_fold_free(IntPtr output);

        /*! \fn structure
         * \brief DllImport from RibosoftAlgo of structure
         * \param candidate Candidate structure
         * \param ideal Ideal structure
         * \param distance Out parameter for the evaluation score
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS structure(string candidate, string ideal, out float distance);

        /*!
         * \brief Default constructor
         */
        public RibosoftAlgo()
        {
        }

        /*! \fn ValidateSequence
         * \brief Algorithm function to validate a sequence
         * \param sequence Sequence being validated
         * \return status Status code
         */
        public R_STATUS ValidateSequence(string sequence)
        {
            return validate_sequence(sequence);
        }

        /*! \fn ValidateStructure
         * \brief Algorithm function to validate a structure
         * \param structure Structure being validated
         * \return status Status code
         */
        public R_STATUS ValidateStructure(string structure)
        {
            return validate_structure(structure);
        }

        /*! \fn Accessibility
         * \brief Algorithm function to determine the accessibility of the cutsite on the input RNA 
         * with this particular candidate sequence and cutsite
         * \param candidate Candidate being evaluated
         * \param rnaStructure structure of input RNA
         * \param cutsiteIndex Cutsite on RNA input (beginning of substrate sequence)
         * \return accessibilityScore Float evaluation score value
        */

        public float Accessibility(Candidate candidate, string rnaStructure, int cutsiteIndex, float naConcentration, float probeConcentration, float targetTemperature)
        {
            string substrateSequence = candidate.SubstrateSequence;
            string substrateStructure = candidate.SubstrateStructure;
            string foldedStructure = rnaStructure.Substring(cutsiteIndex, substrateSequence.Length);

            R_STATUS status = accessibility(substrateSequence, substrateStructure, foldedStructure, naConcentration, probeConcentration, targetTemperature, out float score);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            return score;
        }

        /*! \fn Anneal
         * \brief Algorithm function to determine the annealing temperature of the cutsite on the input RNA with this particular candidate sequence
         * \param candidate Candidate being evaluated
         * \param targetSequence Input RNA of the request
         * \param structure Estimated structure of the ribozyme
         * \param naConcentration Concentration of sodium
         * \param probeConcentration Concentration of probe
         * \param targetTemp Target temperature of binding arms
         * \return temperatureScore Float evaluation score value
         */
        public float Anneal(Candidate candidate, string targetSequence, string structure, float naConcentration, float probeConcentration, float targetTemp)
        {
            float temperatureScore = 0.0f;

            R_STATUS status = anneal(targetSequence, structure, naConcentration, probeConcentration, targetTemp, out float delta);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            temperatureScore = delta;

            return temperatureScore;
        }

        /*! \fn Fold
         * \brief Algorithm function to fold an RNA sequence
         * \param sequence Sequence to be folded
         * \return foldOutputs List of fold outputs, including the structure and its probability
         */
        public IList<FoldOutput> Fold(string sequence, float envTemp)
        {
            R_STATUS status = fold(sequence, envTemp, out IntPtr outputPtr, out int size);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            var currentPtr = outputPtr;
            var foldOutputs = new FoldOutput[size];
            var foldOutputSize = Marshal.SizeOf<FoldOutput>();

            for (int i = 0; i < size; ++i, currentPtr += foldOutputSize)
            {
                foldOutputs[i] = Marshal.PtrToStructure<FoldOutput>(currentPtr);
            }

            fold_output_free(outputPtr, size);

            return foldOutputs;
        }

        /*! \fn MFEFold
         * \brief Algorithm function to fold the input using ViennaRNA's default fold
         * \param sequence Sequence to be folded
         * \return rnaStructure String containing the structure of the folded RNA
        */
        public string MFEFold(string sequence, float envTemp)
        {
            R_STATUS status = mfe_default_fold(sequence, envTemp, out IntPtr structure);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            string rnaStructure = Marshal.PtrToStringAnsi(structure);
            mfe_default_fold_free(structure);

            return rnaStructure;
        }

        /*! \fn Structure
         * \brief Algorithm function to determine the accuracy of the predicted structure to the ideal structure
         * \param designs Designs being evaluated
         * \return void
         */
        public void Structure(IList<Design> designs, float envTemp)
        {
            IList<IList<Tuple<float, float>>> structureResults = new List<IList<Tuple<float, float>>>();

            IList<Tuple<float, float>> currentResults;
            string idealStructure;

            // Store distance and probability for further use, once we have the max distance
            foreach (var d in designs)
            {
                currentResults = new List<Tuple<float, float>>();

                var foldOutputs = Fold(d.Sequence, envTemp);

                idealStructure = d.IdealStructure;

                foreach (var output in foldOutputs)
                {
                    R_STATUS status = structure(output.Structure, idealStructure, out float distance);

                    if (status != R_STATUS.R_STATUS_OK)
                    {
                        throw new RibosoftAlgoException(status);
                    }

                    currentResults.Add(new Tuple<float, float>(distance, output.Probability));
                }

                structureResults.Add(currentResults);
            }

            if (designs.Count != structureResults.Count)
            {
                throw new RibosoftAlgoException(R_STATUS.R_STRUCT_LENGTH_DIFFER);
            }

            float maxDistance = structureResults.Max(results => results.Max(r => r.Item1));

            float currentStructureScore;

            for (int i = 0; i < designs.Count; i++)
            {
                currentStructureScore = 0.0f;

                foreach (Tuple<float, float> foldResults in structureResults[i])
                {
                    currentStructureScore += (1 - (foldResults.Item1 / maxDistance)) * foldResults.Item2;
                }

                designs[i].StructureScore = 1 - currentStructureScore;
            }
        }
    }
}
