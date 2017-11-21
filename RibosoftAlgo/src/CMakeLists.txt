set(SOURCE_FILES
    # Source files here...
    dll.h
    functions.h
    main.cpp)

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

# Find NuGet executable
find_program(MONO_EXECUTABLE mono)

if (NOT MONO_EXECUTABLE)
    message(FATAL_ERROR "mono not found")
endif ()

# Copy library to NuGet package
add_custom_command(TARGET RibosoftAlgo
    POST_BUILD
    COMMAND "${CMAKE_COMMAND}" -E copy 
        "$<TARGET_FILE:RibosoftAlgo>"
        "${NUGET_PACKAGE_OUTPUT_DIR}/$<TARGET_FILE_NAME:RibosoftAlgo>" 
    COMMENT "Copying binary to NuGet directory"
)

# Pack NuGet
add_custom_command(TARGET RibosoftAlgo
    POST_BUILD
    COMMAND "${MONO_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/nuget.exe" pack "${NUGET_PACKAGE_DIR}" -OutputDirectory "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}"
    COMMENT "Creating NuGet package"
)