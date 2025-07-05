cmake_minimum_required(VERSION 3.28)

project(RibosoftAlgo 
    VERSION 2.2.0
    DESCRIPTION "Modern Ribosoft algorithm support library for ribozyme design"
    LANGUAGES CXX)

# Cutting-edge C++23 with modern CMake practices
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Enable modern CMake policies (only if supported)
if(POLICY CMP0135)
    cmake_policy(SET CMP0135 NEW)  # FetchContent timestamp policy
endif()

# Add project dependencies directory to search path
list(APPEND CMAKE_PREFIX_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../.deps")

# Include the project cmake module directory
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

# Set CMake library and binary output directories to /lib and /bin
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

# Platform detection for NuGet packaging
string(TOLOWER ${CMAKE_SYSTEM_NAME} CMAKE_SYSTEM_NAME_LOWER)
if(CMAKE_SYSTEM_NAME_LOWER STREQUAL "darwin")
    set(CMAKE_SYSTEM_NAME_LOWER "osx")
endif()

# NuGet package configuration
set(NUGET_PACKAGE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/nuget)
set(NUGET_PACKAGE_OUTPUT_DIR ${NUGET_PACKAGE_DIR}/build)

# Modern testing with Catch2 - use more reliable FetchContent configuration
include(FetchContent)

# Configure FetchContent to be more robust
set(FETCHCONTENT_QUIET OFF)
set(FETCHCONTENT_BASE_DIR ${CMAKE_BINARY_DIR}/_deps)

# Use tarball download instead of git clone to avoid path issues
FetchContent_Declare(
    Catch2
    URL https://github.com/catchorg/Catch2/archive/refs/tags/v3.5.4.tar.gz
    DOWNLOAD_EXTRACT_TIMESTAMP TRUE
)

# Check if Catch2 is already available
find_package(Catch2 3 QUIET)
if(NOT Catch2_FOUND)
    FetchContent_MakeAvailable(Catch2)
    # Add Catch2 CMake modules to path
    list(APPEND CMAKE_MODULE_PATH ${catch2_SOURCE_DIR}/extras)
endif()

# Modern OpenMP support with macOS compatibility
if(APPLE)
    # On macOS, help CMake find Homebrew-installed libomp
    if(EXISTS "/opt/homebrew/opt/libomp")
        # Apple Silicon Homebrew path
        set(OpenMP_ROOT "/opt/homebrew/opt/libomp")
    elseif(EXISTS "/usr/local/opt/libomp")
        # Intel Mac Homebrew path
        set(OpenMP_ROOT "/usr/local/opt/libomp")
    endif()
endif()

find_package(OpenMP)
if(NOT OpenMP_FOUND)
    message(WARNING "OpenMP not found. Building without OpenMP support.")
    # Create a dummy OpenMP target for compatibility
    add_library(OpenMP::OpenMP_CXX INTERFACE IMPORTED)
else()
    message(STATUS "OpenMP found: ${OpenMP_CXX_VERSION}")
endif()

# Enable testing
enable_testing()

add_subdirectory(src)
add_subdirectory(nuget)
add_subdirectory(test)
