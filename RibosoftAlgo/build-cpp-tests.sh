#!/bin/bash

# C++ test build script for RibosoftAlgo
# Usage: ./build-cpp-tests.sh [linux-x64|win-x64|osx-x64|osx-arm64] [Debug|Release]

set -e

RUNTIME_ID=${1:-linux-x64}
CONFIGURATION=${2:-Release}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$SCRIPT_DIR/bin/$CONFIGURATION/net8.0/runtimes/$RUNTIME_ID/native"

echo "Building RibosoftAlgo C++ tests for $RUNTIME_ID ($CONFIGURATION)"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Test source files
TEST_SOURCES=(
    "$SCRIPT_DIR/test/test_validation.cpp"
    "$SCRIPT_DIR/test/test_anneal.cpp"
    "$SCRIPT_DIR/test/test_fold.cpp"
    "$SCRIPT_DIR/test/test_structure.cpp"
    "$SCRIPT_DIR/test/test_accessibility.cpp"
)

# Main library source files (needed for testing)
LIB_SOURCES=(
    "$SCRIPT_DIR/src/anneal.cpp"
    "$SCRIPT_DIR/src/validation.cpp" 
    "$SCRIPT_DIR/src/fold.cpp"
    "$SCRIPT_DIR/src/structure.cpp"
    "$SCRIPT_DIR/src/accessibility.cpp"
    "$SCRIPT_DIR/src/mfe_default_fold.cpp"
)

# Include paths
INCLUDES=(
    "-I$SCRIPT_DIR/src"
    "-I$SCRIPT_DIR/../.deps/viennarna/include"
    "-I$SCRIPT_DIR/../.deps/melting/include"
)

# Library paths
LIBRARIES=(
    "$SCRIPT_DIR/../.deps/viennarna/lib/libRNA.a"
    "$SCRIPT_DIR/../.deps/melting/lib/libMELTING.a"
)

# Platform-specific compilation
case $RUNTIME_ID in
    linux-x64)
        COMPILER="g++-13"
        CXXFLAGS="-std=c++23 -Wall -Wextra -fopenmp"
        LDFLAGS="-fopenmp"
        OUTPUT_NAME="ribosoft-tests"
        
        if [ "$CONFIGURATION" = "Release" ]; then
            CXXFLAGS="$CXXFLAGS -O3 -march=native -flto -DNDEBUG"
            LDFLAGS="$LDFLAGS -flto"
        else
            CXXFLAGS="$CXXFLAGS -g -O0 -DDEBUG"
        fi
        ;;
        
    win-x64)
        COMPILER="x86_64-w64-mingw32-g++"
        CXXFLAGS="-std=c++23 -Wall -Wextra"
        LDFLAGS=""
        OUTPUT_NAME="ribosoft-tests.exe"
        
        if [ "$CONFIGURATION" = "Release" ]; then
            CXXFLAGS="$CXXFLAGS -O3 -DNDEBUG"
        else
            CXXFLAGS="$CXXFLAGS -g -O0 -DDEBUG"
        fi
        ;;
        
    osx-x64)
        COMPILER="clang++"
        CXXFLAGS="-std=c++23 -Wall -Wextra -Xpreprocessor -fopenmp"
        LDFLAGS="-lomp"
        OUTPUT_NAME="ribosoft-tests"
        
        if [ "$CONFIGURATION" = "Release" ]; then
            CXXFLAGS="$CXXFLAGS -O3 -march=native -flto -DNDEBUG"
            LDFLAGS="$LDFLAGS -flto"
        else
            CXXFLAGS="$CXXFLAGS -g -O0 -DDEBUG"
        fi
        ;;
        
    osx-arm64)
        COMPILER="clang++"
        CXXFLAGS="-std=c++23 -Wall -Wextra -Xpreprocessor -fopenmp -target arm64-apple-macos11"
        LDFLAGS="-lomp -target arm64-apple-macos11"
        OUTPUT_NAME="ribosoft-tests"
        
        if [ "$CONFIGURATION" = "Release" ]; then
            CXXFLAGS="$CXXFLAGS -O3 -mcpu=apple-m1 -flto -DNDEBUG"
            LDFLAGS="$LDFLAGS -flto"
        else
            CXXFLAGS="$CXXFLAGS -g -O0 -DDEBUG"
        fi
        ;;
        
    *)
        echo "Unsupported runtime identifier: $RUNTIME_ID"
        exit 1
        ;;
esac

# Check if compiler exists
if ! command -v "$COMPILER" &> /dev/null; then
    echo "Compiler $COMPILER not found. Please install the required toolchain."
    exit 1
fi

# Download and setup Catch2 if not present
CATCH2_HEADER="$SCRIPT_DIR/test/catch2/catch_amalgamated.hpp"
CATCH2_SOURCE="$SCRIPT_DIR/test/catch2/catch_amalgamated.cpp"

if [ ! -f "$CATCH2_HEADER" ] || [ ! -f "$CATCH2_SOURCE" ]; then
    echo "Downloading Catch2 v3.5.4..."
    mkdir -p "$SCRIPT_DIR/test/catch2"
    
    curl -L -o "$CATCH2_HEADER" "https://github.com/catchorg/Catch2/releases/download/v3.5.4/catch_amalgamated.hpp"
    curl -L -o "$CATCH2_SOURCE" "https://github.com/catchorg/Catch2/releases/download/v3.5.4/catch_amalgamated.cpp"
fi

# Add Catch2 to includes and sources
INCLUDES+=("-I$SCRIPT_DIR/test")
ALL_SOURCES=("${TEST_SOURCES[@]}" "${LIB_SOURCES[@]}" "$CATCH2_SOURCE")

# Build command
BUILD_CMD="$COMPILER $CXXFLAGS ${INCLUDES[*]} ${ALL_SOURCES[*]} ${LIBRARIES[*]} $LDFLAGS -o $OUTPUT_DIR/$OUTPUT_NAME"

echo "Executing: $BUILD_CMD"
eval "$BUILD_CMD"

if [ $? -eq 0 ]; then
    echo "✅ Successfully built $OUTPUT_NAME for $RUNTIME_ID"
    echo "📁 Output: $OUTPUT_DIR/$OUTPUT_NAME"
    
    # Make executable
    chmod +x "$OUTPUT_DIR/$OUTPUT_NAME"
    
    # Verify the executable
    if [ -f "$OUTPUT_DIR/$OUTPUT_NAME" ]; then
        echo "📊 Executable size: $(du -h "$OUTPUT_DIR/$OUTPUT_NAME" | cut -f1)"
    fi
else
    echo "❌ Build failed for $RUNTIME_ID"
    exit 1
fi
