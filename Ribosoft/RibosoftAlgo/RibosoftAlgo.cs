using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
         * \param substrateTemplate Template of the substrate
         * \param cutsiteIndex Index of the cutsite
         * \param cutsiteNumber Number of the cutsite
         * \param delta Out parameter for the evaluation score
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS accessibility(string substrateSequence, out string rnaStructure);

        /*! \fn anneal
         * \brief DllImport from RibosoftAlgo of anneal
         * \param sequence RNA sequence
         * \param structure RNA structure
         * \param na_concentration Concentration of sodium
         * \param probe_concentration Concentration of probe
         * \param temp Out parameter for the evaluation score
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS anneal(string sequence, string structure, float na_concentration, float probe_concentration, out float temp);

        /*! \fn fold
         * \brief DllImport from RibosoftAlgo of fold
         * \param sequence RNA sequence
         * \param output Output pointer to the list of fold outputs
         * \param size Out value of the size of the list
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS fold(string sequence, out IntPtr output, out int size);

        /*! \fn fold_output_free
         * \brief DllImport from RibosoftAlgo of fold_output_free
         * \param output Pointer to fold output list
         * \param size Size of the list
         */
        [DllImport("RibosoftAlgo")]
        private static extern void fold_output_free(IntPtr output, int size);

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

        /*! \fn RNAFolding
         * \brief Algorithm function to fold the full input RNA 
         * \param candidate Candidate being evaluated
         * \param rnaInput Input RNA of the request
         * \param cutsiteNumber Initial cutsite location for ribozyme template
         * \param cutsiteIndex Cutsite on RNA input
         * \return accessibilityScore Float evaluation score value
         */
        public string RNAFolding(string rnaInput)
        {
            R_STATUS status = accessibility(rnaInput, out string rnaStructure);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            return rnaStructure;

        }

        /*! \fn Accessibility
         * \brief Algorithm function to determine the accessibility of the cutsite on the input RNA 
         * with this particular candidate sequence and cutsite
         * \param candidate Candidate being evaluated
         * \param rnaInput Input RNA of the request
         * \param cutsiteNumber Initial cutsite location for ribozyme template
         * \param cutsiteIndex Cutsite on RNA input
         * \return accessibilityScore Float evaluation score value
         */
        public float Accessibility(Candidate candidate, string rnaInput, int cutsiteNumber, int cutsiteIndex, float naConcentration, float probeConcentration, string rnaStructure)
        {
            string substrateSequence = candidate.SubstrateStructure;
            string foldStructure = rnaStructure.Substring(cutsiteIndex, candidate.SubstrateSequence.Length);

            Regex r = new Regex("[0-9a-zA-Z]+");

            bool isSingleStranded = true;

            foreach (Match match in r.Matches(substrateSequence))
            {
                if (foldStructure[match.Index] != '.')
                {
                    isSingleStranded = false;
                    break;
                }
            }

            if (isSingleStranded)
            {
                return 0.0f;
            }
            else
            {
                return Anneal(candidate, substrateSequence, foldStructure, naConcentration, probeConcentration);
            }
        }

        /*! \fn Anneal
         * \brief Algorithm function to determine the annealing temperature of the cutsite on the input RNA with this particular candidate sequence
         * \param candidate Candidate being evaluated
         * \param targetSequence Input RNA of the request
         * \param structure Estimated structure of the ribozyme
         * \param naConcentration Concentration of sodium
         * \param probeConcentration Concentration of probe
         * \return temperatureScore Float evaluation score value
         */
        public float Anneal(Candidate candidate, string targetSequence, string structure, float naConcentration, float probeConcentration)
        {
            float temperatureScore = 0.0f;

            R_STATUS status = anneal(targetSequence, structure, naConcentration, probeConcentration, out float delta);

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
        public IList<FoldOutput> Fold(string sequence)
        {
            R_STATUS status = fold(sequence, out IntPtr outputPtr, out int size);

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

        /*! \fn Structure
         * \brief Algorithm function to determine the accuracy of the predicted structure to the ideal structure
         * \param candidate Candidate being evaluated
         * \param ideal Ideal ribozyme structure
         * \return structureScore Float evaluation score value
         */
        public float Structure(Candidate candidate, string ideal)
        {
            float structureScore = 0.0f;

            var foldOutputs = Fold(candidate.Sequence.GetString());

            foreach (var output in foldOutputs)
            {
                R_STATUS status = structure(output.Structure, ideal, out float distance);

                if (status != R_STATUS.R_STATUS_OK)
                {
                    throw new RibosoftAlgoException(status);
                }

                structureScore += distance * output.Probability;
            }

            return structureScore;
        }
    }
}
