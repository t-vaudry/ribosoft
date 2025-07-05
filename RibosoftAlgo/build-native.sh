#!/bin/bash

# Cross-platform native library build script for RibosoftAlgo
# Usage: ./build-native.sh [linux-x64|win-x64|osx-x64|osx-arm64] [Debug|Release]

set -e

RUNTIME_ID=${1:-linux-x64}
CONFIGURATION=${2:-Release}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$SCRIPT_DIR/bin/$CONFIGURATION/net8.0/runtimes/$RUNTIME_ID/native"

echo "Building RibosoftAlgo native library for $RUNTIME_ID ($CONFIGURATION)"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Source files
SOURCES=(
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
        CXXFLAGS="-std=c++23 -fPIC -shared -Wall -Wextra -fopenmp"
        LDFLAGS="-fopenmp"
        OUTPUT_NAME="libRibosoftAlgo.so"
        
        if [ "$CONFIGURATION" = "Release" ]; then
            CXXFLAGS="$CXXFLAGS -O3 -march=native -flto -DNDEBUG"
            LDFLAGS="$LDFLAGS -flto"
        else
            CXXFLAGS="$CXXFLAGS -g -O0 -DDEBUG"
        fi
        ;;
        
    win-x64)
        # Note: This would require MinGW-w64 or cross-compilation setup
        COMPILER="x86_64-w64-mingw32-g++"
        CXXFLAGS="-std=c++23 -shared -Wall -Wextra"
        LDFLAGS=""
        OUTPUT_NAME="RibosoftAlgo.dll"
        
        if [ "$CONFIGURATION" = "Release" ]; then
            CXXFLAGS="$CXXFLAGS -O3 -DNDEBUG"
        else
            CXXFLAGS="$CXXFLAGS -g -O0 -DDEBUG"
        fi
        ;;
        
    osx-x64)
        COMPILER="clang++"
        CXXFLAGS="-std=c++23 -fPIC -shared -Wall -Wextra -Xpreprocessor -fopenmp"
        LDFLAGS="-lomp"
        OUTPUT_NAME="libRibosoftAlgo.dylib"
        
        if [ "$CONFIGURATION" = "Release" ]; then
            CXXFLAGS="$CXXFLAGS -O3 -march=native -flto -DNDEBUG"
            LDFLAGS="$LDFLAGS -flto"
        else
            CXXFLAGS="$CXXFLAGS -g -O0 -DDEBUG"
        fi
        ;;
        
    osx-arm64)
        COMPILER="clang++"
        CXXFLAGS="-std=c++23 -fPIC -shared -Wall -Wextra -Xpreprocessor -fopenmp -target arm64-apple-macos11"
        LDFLAGS="-lomp -target arm64-apple-macos11"
        OUTPUT_NAME="libRibosoftAlgo.dylib"
        
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

# Build command
BUILD_CMD="$COMPILER $CXXFLAGS ${INCLUDES[*]} ${SOURCES[*]} ${LIBRARIES[*]} $LDFLAGS -o $OUTPUT_DIR/$OUTPUT_NAME"

echo "Executing: $BUILD_CMD"
eval "$BUILD_CMD"

if [ $? -eq 0 ]; then
    echo "✅ Successfully built $OUTPUT_NAME for $RUNTIME_ID"
    echo "📁 Output: $OUTPUT_DIR/$OUTPUT_NAME"
    
    # Verify the library
    if [ -f "$OUTPUT_DIR/$OUTPUT_NAME" ]; then
        echo "📊 Library size: $(du -h "$OUTPUT_DIR/$OUTPUT_NAME" | cut -f1)"
        
        # Show dependencies (Linux/macOS)
        if command -v ldd &> /dev/null && [ "$RUNTIME_ID" = "linux-x64" ]; then
            echo "🔗 Dependencies:"
            ldd "$OUTPUT_DIR/$OUTPUT_NAME" | head -10
        elif command -v otool &> /dev/null && [[ "$RUNTIME_ID" == osx-* ]]; then
            echo "🔗 Dependencies:"
            otool -L "$OUTPUT_DIR/$OUTPUT_NAME"
        fi
    fi
else
    echo "❌ Build failed for $RUNTIME_ID"
    exit 1
fi
