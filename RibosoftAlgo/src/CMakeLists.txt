set(SOURCE_FILES
    # Source files here...
    main.cpp
    dll.h)

add_definitions(-DBUILDING_DLL)

# Create the library
add_library(RibosoftAlgo SHARED ${SOURCE_FILES})

# Link required libraries
target_link_libraries(RibosoftAlgo ${LIBRARIES})

# Mark this directory as a root directory for future includes
target_include_directories(RibosoftAlgo PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})

# Find NuGet executable
find_program(NUGET_EXECUTABLE nuget HINTS "${CMAKE_SOURCE_DIR}")

if (NOT NUGET_EXECUTABLE)
    message(FATAL_ERROR "NuGet not found")
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
    COMMAND "${NUGET_EXECUTABLE}" pack "${NUGET_PACKAGE_DIR}" -OutputDirectory "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}"
    COMMENT "Creating NuGet package"
)