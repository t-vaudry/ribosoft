using System;
using System.Runtime.InteropServices;

namespace Ribosoft
{
    public class SampleDllCall
    {
        [DllImport("RibosoftAlgo")]
        extern static int math_add(int a, int b);

        [DllImport("RibosoftAlgo")]
        extern static R_ERROR validate_sequence(String sequence);

        public SampleDllCall()
        {
        }

        public int Add(int a, int b)
        {
            return math_add(a, b);
        }

        public R_ERROR ValidateSequence(String seq)
        {
            return validate_sequence(seq);
        }
    }
}
