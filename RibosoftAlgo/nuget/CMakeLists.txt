add_custom_target(nuget ALL DEPENDS RibosoftAlgo)

set(NUGET_EXECUTABLE "${CMAKE_SOURCE_DIR}/nuget.exe")

# If we're not on Windows, find mono to run nuget.exe
if (NOT WIN32)
    # Find NuGet executable
    find_program(MONO_EXECUTABLE mono)

    if (MONO_EXECUTABLE-NOTFOUND)
        message(FATAL_ERROR "mono not found (install mono-complete)")
    endif ()
endif (NOT WIN32)

# Find dotnet executable
find_program(DOTNET_EXECUTABLE dotnet)

if (DOTNET_EXECUTABLE-NOTFOUND)
    message(WARNING "dotnet not found (install dotnet-sdk)")
endif ()

# Copy library to NuGet package
add_custom_command(TARGET nuget
    POST_BUILD
    COMMAND "${CMAKE_COMMAND}" -E copy
        "$<TARGET_FILE:RibosoftAlgo>"
        "${NUGET_PACKAGE_OUTPUT_DIR}/$<TARGET_FILE_NAME:RibosoftAlgo>"
    COMMENT "Copying binary to NuGet directory"
)

# Pack NuGet
if (WIN32)
    add_custom_command(TARGET nuget
        POST_BUILD
        COMMAND "${NUGET_EXECUTABLE}" pack "${NUGET_PACKAGE_DIR}" -OutputDirectory "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}"
        COMMENT "Creating NuGet package"
    )
else()
    add_custom_command(TARGET nuget
        POST_BUILD
        COMMAND "${MONO_EXECUTABLE}" "${NUGET_EXECUTABLE}" pack "${NUGET_PACKAGE_DIR}" -OutputDirectory "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}"
        COMMENT "Creating NuGet package"
    )
endif()

# Clear dotnet nuget cache
if (DOTNET_EXECUTABLE)
    add_custom_command(TARGET nuget
        POST_BUILD
        COMMAND "${DOTNET_EXECUTABLE}" nuget locals all --clear
        COMMENT "Clearing dotnet nuget cache"
    )
endif ()