#!/bin/bash

# Comprehensive Coverage Test Runner for Ribosoft
# This script runs all test suites and generates coverage reports

set -e

echo "🧪 Ribosoft Comprehensive Coverage Analysis"
echo "==========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Cleanup function
cleanup() {
    print_status "Cleaning up temporary files..."
    find . -name "*.gcda" -delete 2>/dev/null || true
    find . -name "*.gcno" -delete 2>/dev/null || true
}

# Set up cleanup trap
trap cleanup EXIT

# Start timing
start_time=$(date +%s)

print_status "Starting comprehensive coverage analysis..."

# 1. C++ Tests with Coverage
print_status "Running C++ tests with coverage instrumentation..."
cd RibosoftAlgo.Tests

if ENABLE_COVERAGE=true ./build-cpp-tests.sh linux-x64 Debug; then
    print_success "C++ tests built successfully"
    
    if ./bin/Debug/net8.0/runtimes/linux-x64/native/ribosoft-tests; then
        print_success "C++ tests executed successfully"
        
        # Generate coverage report
        if lcov --capture --directory ./bin/Debug/net8.0/runtimes/linux-x64/native --output-file coverage-cpp.info --ignore-errors gcov,source; then
            lcov --remove coverage-cpp.info '*/test/*' '*/usr/*' --output-file coverage-cpp-filtered.info --ignore-errors unused
            genhtml coverage-cpp-filtered.info --output-directory coverage-cpp-html
            print_success "C++ coverage report generated in coverage-cpp-html/"
        else
            print_error "Failed to generate C++ coverage report"
        fi
    else
        print_error "C++ tests failed"
    fi
else
    print_error "Failed to build C++ tests"
fi

cd ..

# 2. C# Tests with Coverage
print_status "Running C# tests with coverage..."

if dotnet test Ribosoft.sln --settings coverlet.runsettings --collect:"XPlat Code Coverage" --results-directory ./TestResults; then
    print_success "C# tests completed successfully"
    
    # Generate HTML report
    export PATH="$PATH:/home/thomas/.dotnet/tools"
    if reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"coverage-csharp-html" -reporttypes:"Html;Cobertura"; then
        print_success "C# coverage report generated in coverage-csharp-html/"
    else
        print_error "Failed to generate C# coverage report"
    fi
else
    print_error "C# tests failed"
fi

# 3. Python Tests with Coverage
print_status "Running Python tests with coverage..."

if source venv313/bin/activate && python -m pytest test_ribosoft.py --cov=ribosoft --cov-report=html:coverage-python-html --cov-report=xml:coverage-python.xml -v; then
    print_success "Python tests completed successfully"
    print_success "Python coverage report generated in coverage-python-html/"
else
    print_error "Python tests failed"
fi

# 4. Robot Framework Tests (if possible)
print_status "Running Robot Framework integration tests..."
cd test

if source ../venv313/bin/activate && robot --outputdir reports tests/startup.robot; then
    print_success "Robot Framework tests completed"
else
    print_warning "Robot Framework tests had issues (may be expected without Docker)"
fi

cd ..

# 5. Generate Summary Report
print_status "Generating coverage summary..."

# Extract coverage percentages
cpp_coverage=$(grep -o "lines......: [0-9.]*%" RibosoftAlgo.Tests/coverage-cpp-filtered.info | head -1 | grep -o "[0-9.]*" || echo "N/A")
csharp_line_coverage=$(grep -A 1 "Line coverage:" coverage-csharp-html/index.html | grep -o "[0-9.]*%" | head -1 || echo "N/A")
csharp_branch_coverage=$(grep -A 1 "Branch coverage:" coverage-csharp-html/index.html | grep -o "[0-9.]*%" | head -1 || echo "N/A")
python_coverage=$(grep -o 'line-rate="[0-9.]*"' coverage-python.xml | head -1 | grep -o "[0-9.]*" | awk '{printf "%.1f", $1*100}' || echo "N/A")

# Calculate end time
end_time=$(date +%s)
duration=$((end_time - start_time))

echo ""
echo "📊 COVERAGE SUMMARY REPORT"
echo "=========================="
echo "Execution Time: ${duration} seconds"
echo ""
echo "C++ Core Algorithm Library:"
echo "  Line Coverage: ${cpp_coverage}%"
echo "  Report: RibosoftAlgo.Tests/coverage-cpp-html/index.html"
echo ""
echo "C# Web Application:"
echo "  Line Coverage: ${csharp_line_coverage}"
echo "  Branch Coverage: ${csharp_branch_coverage}"
echo "  Report: coverage-csharp-html/index.html"
echo ""
echo "Python CLI Tool:"
echo "  Line Coverage: ${python_coverage}%"
echo "  Report: coverage-python-html/index.html"
echo ""
echo "📋 Detailed Analysis:"
echo "  See COVERAGE_ANALYSIS.md for comprehensive analysis"
echo ""

# Check for critical issues
if [[ "$cpp_coverage" == "N/A" ]] || [[ $(echo "$cpp_coverage < 90" | bc -l) -eq 1 ]]; then
    print_warning "C++ coverage is below 90% - review critical gaps"
fi

if [[ "$python_coverage" == "N/A" ]] || [[ $(echo "$python_coverage < 60" | bc -l) -eq 1 ]]; then
    print_warning "Python coverage is below 60% - significant gaps exist"
fi

print_success "Coverage analysis complete! Check the generated reports."
echo ""
echo "🔗 Quick Links:"
echo "  C++ Report:    file://$(pwd)/RibosoftAlgo.Tests/coverage-cpp-html/index.html"
echo "  C# Report:     file://$(pwd)/coverage-csharp-html/index.html"
echo "  Python Report: file://$(pwd)/coverage-python-html/index.html"
echo "  Analysis:      $(pwd)/COVERAGE_ANALYSIS.md"
