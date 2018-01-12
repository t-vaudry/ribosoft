using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ribosoft
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct FoldOutput
    {
        public string Structure;
        public float Energy;
    }

    public class RibosoftAlgo
    {
        [DllImport("RibosoftAlgo")]
        private static extern int math_add(int a, int b);

        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS validate_sequence(string sequence);

        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS validate_structure(string structure);

        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS fold(string sequence, out IntPtr output, out int size);

        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS structure(string candidate, string ideal, out float distance);

        [DllImport("RibosoftAlgo")]
        private static extern R_STATUS accessibility(string substrateSequence, string substrateTemplate, int cutsiteIndex, int cutsiteNumber, out float delta);

        public RibosoftAlgo()
        {
        }

        public int Add(int a, int b)
        {
            return math_add(a, b);
        }

        public R_STATUS ValidateSequence(string sequence)
        {
            return validate_sequence(sequence);
        }

        public R_STATUS ValidateStructure(string structure)
        {
            return validate_structure(structure);
        }

        public IList<FoldOutput> Fold(string sequence)
        {
            R_STATUS status = fold(sequence, out IntPtr outputPtr, out int size);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            var foldOutputs = new FoldOutput[size];

            for (int i = 0; i < size; ++i, outputPtr += Marshal.SizeOf<FoldOutput>())
            {
                foldOutputs[i] = Marshal.PtrToStructure<FoldOutput>(outputPtr);
            }

            return foldOutputs;
        }

        public float Structure(string candidate, string ideal)
        {
            R_STATUS status = structure(candidate, ideal, out float distance);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            return distance;
        }

        public float Accessibility(string substrateSequence, string substrateTemplate, int cutsiteIndex, int cutsiteNumber)
        {
            R_STATUS status = accessibility(substrateSequence, substrateTemplate, cutsiteIndex, cutsiteNumber, out float delta);

            if (status != R_STATUS.R_STATUS_OK)
            {
                throw new RibosoftAlgoException(status);
            }

            return delta;
        }
    }

    public class RibosoftAlgoException : Exception
    {
        public R_STATUS Code { get; set; }

        public RibosoftAlgoException(R_STATUS code)
        {
            this.Code = code;
        }

        public RibosoftAlgoException()
            : this(R_STATUS.R_APPLICATION_ERROR_LAST)
        {
        }
    }
}
