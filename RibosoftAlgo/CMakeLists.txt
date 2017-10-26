cmake_minimum_required(VERSION 3.2)

project(RibosoftAlgo)

# Include the project cmake module directory
list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

# Set CMake library and binary output directories to /lib and /bin
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

set(NUGET_PACKAGE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/nuget)
set(NUGET_PACKAGE_OUTPUT_DIR ${NUGET_PACKAGE_DIR}/build)

# Find includes in corresponding build directories
set(CMAKE_INCLUDE_CURRENT_DIR ON)

# Use C++11
set(CMAKE_CXX_STANDARD 11)

add_subdirectory(src)