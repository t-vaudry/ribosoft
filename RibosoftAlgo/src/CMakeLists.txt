# Source files
set(SOURCE_FILES
    dll.h
    functions.h
    error.h
    anneal.cpp
    validation.cpp
    fold.cpp
    structure.cpp
    accessibility.cpp
    mfe_default_fold.cpp
)

# Find ViennaRNA/RNAlib
find_package(RNA REQUIRED)

# Find MELTING V4.3
find_package(MELTING REQUIRED)

# Create the shared library
add_library(RibosoftAlgo SHARED ${SOURCE_FILES})

# Modern CMake target properties with C++23 optimizations
target_compile_definitions(RibosoftAlgo PRIVATE BUILDING_DLL)

# C++23 specific compiler features and optimizations
target_compile_features(RibosoftAlgo 
    PUBLIC 
        cxx_std_23
)

# Enhanced compiler flags for C++23 and performance
target_compile_options(RibosoftAlgo 
    PRIVATE
        $<$<CXX_COMPILER_ID:GNU>:-Wall -Wextra -O3 -march=native -flto>
        $<$<CXX_COMPILER_ID:Clang>:-Wall -Wextra -O3 -march=native -flto>
        $<$<CXX_COMPILER_ID:MSVC>:/W4 /O2 /GL>
)

# Link-time optimization for better performance
set_target_properties(RibosoftAlgo PROPERTIES
    INTERPROCEDURAL_OPTIMIZATION TRUE
)

# Link required libraries using modern CMake
target_link_libraries(RibosoftAlgo 
    PRIVATE 
        ${RNA_LIBRARY} 
        ${MELTING_LIBRARY}
)

# Always try to link OpenMP if available
if(OpenMP_FOUND)
    target_link_libraries(RibosoftAlgo PRIVATE OpenMP::OpenMP_CXX)
endif()

# Include directories using modern CMake
target_include_directories(RibosoftAlgo 
    PUBLIC 
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>
        $<INSTALL_INTERFACE:include>
    PRIVATE
        ${RNA_INCLUDE_DIR}
        ${MELTING_INCLUDE_DIR}
)

# Set library properties with enhanced versioning
set_target_properties(RibosoftAlgo PROPERTIES
    VERSION ${PROJECT_VERSION}
    SOVERSION ${PROJECT_VERSION_MAJOR}
    CXX_VISIBILITY_PRESET hidden
    VISIBILITY_INLINES_HIDDEN ON
    OUTPUT_NAME "RibosoftAlgo"
    EXPORT_NAME "RibosoftAlgo"
)