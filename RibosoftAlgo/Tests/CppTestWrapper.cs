using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace RibosoftAlgo.Tests
{
    /// <summary>
    /// .NET test wrapper that executes the native C++ Catch2 tests
    /// and integrates them with the dotnet test framework
    /// </summary>
    public class CppTestWrapper
    {
        [Fact]
        public void RunAllCppTests()
        {
            string testExecutableName = GetTestExecutableName();
            string testExecutablePath = GetTestExecutablePath(testExecutableName);
            
            Assert.True(File.Exists(testExecutablePath), 
                $"C++ test executable not found at: {testExecutablePath}. Make sure the C++ tests were built successfully.");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = testExecutablePath,
                Arguments = "--reporter=junit --out=test-results.xml", // Generate JUnit XML for better integration
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            Assert.NotNull(process);
            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            process.WaitForExit();
            
            // Output the C++ test results for visibility
            if (!string.IsNullOrEmpty(output))
            {
                Console.WriteLine("C++ Test Output:");
                Console.WriteLine(output);
            }
            
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("C++ Test Errors:");
                Console.WriteLine(error);
            }
            
            Assert.True(process.ExitCode == 0, 
                $"C++ tests failed with exit code {process.ExitCode}. Check the output above for details.");
        }
        
        private static string GetTestExecutableName()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? "ribosoft-tests.exe" 
                : "ribosoft-tests";
        }
        
        private static string GetTestExecutablePath(string executableName)
        {
            string runtimeId = GetRuntimeIdentifier();
            string configuration = GetConfiguration();
            
            string basePath = AppContext.BaseDirectory;
            string relativePath = Path.Combine("runtimes", runtimeId, "native", executableName);
            
            return Path.Combine(basePath, relativePath);
        }
        
        private static string GetRuntimeIdentifier()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "win-x64";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
            else
                return "linux-x64";
        }
        
        private static string GetConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
