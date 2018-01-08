using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestFolding
    {
        [Fact]
        public void TestFolding_Valid()
        {
            SampleDllCall sdc = new SampleDllCall();

            IntPtr outputPtr;
            int size;

            sdc.Fold("AUGUCUUAGGUGAUACGUGC", out outputPtr, out size);

            Assert.False(outputPtr == IntPtr.Zero);

            FoldOutput[] decodedData = new FoldOutput[size];

            for (int i = 0; i < size; ++i, outputPtr += Marshal.SizeOf<FoldOutput>())
            {
                decodedData[i] = Marshal.PtrToStructure<FoldOutput>(outputPtr);
            }

            Assert.Equal(".((((......)))).....", decodedData[0].Structure);
            Assert.Equal(-1.60f, decodedData[0].Energy);
            Assert.Equal("((((..(.....).))))..", decodedData[35].Structure);
            Assert.Equal(3.00f, decodedData[35].Energy);
            Assert.Equal(51, size);
        }
    }
}