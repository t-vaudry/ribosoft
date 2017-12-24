set(SOURCE_FILES
    # Source files here...
    dll.h
    functions.h
    error.h
    main.cpp
    anneal.cpp
    validation.cpp
    )

add_definitions(-DBUILDING_DLL)

# Find ViennaRNA/RNAlib
find_package(RNA REQUIRED)
include_directories(${RNA_INCLUDE_DIR})

# Create the library
add_library(RibosoftAlgo SHARED ${SOURCE_FILES})

# Link required libraries
target_link_libraries(RibosoftAlgo ${LIBRARIES} ${RNA_LIBRARY})

# Mark this directory as a root directory for future includes
target_include_directories(RibosoftAlgo PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})