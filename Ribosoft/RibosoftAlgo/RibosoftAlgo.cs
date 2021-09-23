using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
         * \param substrateTemplate Template of the substrate
         * \param cutsiteIndex Index of the cutsite
         * \param cutsiteNumber Number of the cutsite
         * \param delta Out parameter for the evaluation score
         * \return status Status code
         */
        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS accessibility(string substrateSequence, string substrateTemplate, int cutsiteIndex, int cutsiteNumber, out float delta);

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

        /*! \fn Accessibility
         * \brief Algorithm function to determine the accessibility of the cutsite on the input RNA with this particular candidate sequence
         * \param candidate Candidate being evaluated
         * \param rnaInput Input RNA of the request
         * \param cutsiteNumber Initial cutsite location for ribozyme template
         * \return accessibilityScore Float evaluation score value
         */
        public float Accessibility(Candidate candidate, string rnaInput, int cutsiteNumber)
        {
            float accessibilityScore = 0.0f;

            foreach (var cutsiteIndex in candidate.CutsiteIndices)
            {
                R_STATUS status = accessibility(rnaInput, candidate.SubstrateSequence, cutsiteIndex, cutsiteNumber, out float delta);

                if (status != R_STATUS.R_STATUS_OK)
                {
                    throw new RibosoftAlgoException(status);
                }

                accessibilityScore += delta;
            }

            return accessibilityScore;
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
         * \param designs Designs being evaluated
         * \return void
         */
        public void Structure(IList<Design> designs)
        {
            IList<IList<Tuple<float, float>>> structureResults = new List<IList<Tuple<float, float>>>();

            IList<Tuple<float, float>> currentResults;
            string idealStructure;

            // Store distance and probability for further use, once we have the max distance
            foreach (var d in designs)
            {
                currentResults = new List<Tuple<float, float>>();

                var foldOutputs = Fold(d.Sequence);

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
