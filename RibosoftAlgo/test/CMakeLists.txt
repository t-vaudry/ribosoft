set(TEST_SOURCES
    # Sources here...
    test_validation.cpp
    test_structure.cpp
    main.cpp)

add_executable(tests ${TEST_SOURCES})
target_link_libraries(tests Catch RibosoftAlgo)

# CTest integration
set(PARSE_CATCH_TESTS_VERBOSE ON)
set(PARSE_CATCH_TESTS_ADD_TO_CONFIGURE_DEPENDS ON)
include(ParseAndAddCatchTests)
ParseAndAddCatchTests(tests)