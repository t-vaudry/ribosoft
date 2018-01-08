using System;
using System.Runtime.InteropServices;

namespace Ribosoft
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct FoldOutput
    {
        public string Structure;
        public float Energy;
    }

    public class SampleDllCall
    {
        [DllImport("RibosoftAlgo")]
        extern static int math_add(int a, int b);

        [DllImport("RibosoftAlgo")]
        extern static R_STATUS validate_sequence(String sequence);

        [DllImport("RibosoftAlgo")]
        extern static R_STATUS fold(String sequence, out IntPtr output, out int size);

        public SampleDllCall()
        {
        }

        public int Add(int a, int b)
        {
            return math_add(a, b);
        }

        public R_STATUS ValidateSequence(String seq)
        {
            return validate_sequence(seq);
        }

        public R_STATUS Fold(String sequence, out IntPtr output, out int size)
        {
            return fold(sequence, out output, out size);
        }
    }
}
