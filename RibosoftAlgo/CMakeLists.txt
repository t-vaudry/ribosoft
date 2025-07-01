cmake_minimum_required(VERSION 3.20)

project(RibosoftAlgo 
    VERSION 2.0.0
    DESCRIPTION "Modern Ribosoft algorithm support library for ribozyme design"
    LANGUAGES CXX)

# Cutting-edge C++23 with modern CMake practices
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Enable modern CMake policies
cmake_policy(SET CMP0135 NEW)  # FetchContent timestamp policy

# Add project dependencies directory to search path
list(APPEND CMAKE_PREFIX_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../.deps")

# Include the project cmake module directory
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

# Set CMake library and binary output directories to /lib and /bin
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

# NuGet package configuration
set(NUGET_PACKAGE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/nuget)
set(NUGET_PACKAGE_OUTPUT_DIR ${NUGET_PACKAGE_DIR}/build)

# Modern testing with latest Catch2
include(FetchContent)
FetchContent_Declare(
    Catch2
    GIT_REPOSITORY https://github.com/catchorg/Catch2.git
    GIT_TAG v3.5.4  # Latest stable version
    FIND_PACKAGE_ARGS NAMES Catch2
)
FetchContent_MakeAvailable(Catch2)

# Add Catch2 CMake modules to path
list(APPEND CMAKE_MODULE_PATH ${catch2_SOURCE_DIR}/extras)

# Modern OpenMP support
find_package(OpenMP REQUIRED)

# Enable testing
enable_testing()

add_subdirectory(src)
add_subdirectory(nuget)
add_subdirectory(test)
