using System;
using System.Runtime.InteropServices;

namespace Ribosoft
{
    public class SampleDllCall
    {
        [DllImport("RibosoftAlgo")]
        extern static int math_add(int a, int b);

        public SampleDllCall()
        {
        }

        public int Add(int a, int b)
        {
            return math_add(a, b);
        }
    }
}
