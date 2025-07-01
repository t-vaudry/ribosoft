# Test source files
set(TEST_SOURCES
    test_anneal.cpp
    test_fold.cpp
    test_validation.cpp
    test_structure.cpp
    test_accessibility.cpp
)

# Create test executable with modern features
add_executable(tests ${TEST_SOURCES})

# Ensure C++23 for tests
target_compile_features(tests PRIVATE cxx_std_23)

# Enhanced test compilation flags
target_compile_options(tests 
    PRIVATE
        $<$<CXX_COMPILER_ID:GNU>:-Wall -Wextra -O2 -g>
        $<$<CXX_COMPILER_ID:Clang>:-Wall -Wextra -O2 -g>
        $<$<CXX_COMPILER_ID:MSVC>:/W4 /O2>
)

# Link with modern Catch2 and our library
target_link_libraries(tests 
    PRIVATE 
        Catch2::Catch2WithMain
        RibosoftAlgo
)

# Modern CTest integration with Catch2 v3 (simplified)
include(Catch)
catch_discover_tests(tests
    TEST_PREFIX "RibosoftAlgo::"
)