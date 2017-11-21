cmake_minimum_required(VERSION 3.2)

project(RibosoftAlgo)

# Add project dependencies directory to search path
list(APPEND CMAKE_PREFIX_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../.deps")

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

# Catch library
set(CATCH_INCLUDE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/include/catch)
add_library(Catch INTERFACE)
target_include_directories(Catch INTERFACE ${CATCH_INCLUDE_DIR})
enable_testing()

add_subdirectory(src)
add_subdirectory(nuget)
add_subdirectory(test)