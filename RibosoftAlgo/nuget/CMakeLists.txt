add_custom_target(nuget ALL DEPENDS RibosoftAlgo)

# Find dotnet executable
find_program(DOTNET_EXECUTABLE dotnet)

if (NOT DOTNET_EXECUTABLE)
    message(FATAL_ERROR "dotnet not found (install dotnet-sdk-8.0 or later)")
endif ()

# Copy library to NuGet package directory
add_custom_command(TARGET nuget
    POST_BUILD
    COMMAND "${CMAKE_COMMAND}" -E copy
        "$<TARGET_FILE:RibosoftAlgo>"
        "${NUGET_PACKAGE_OUTPUT_DIR}/$<TARGET_FILE_NAME:RibosoftAlgo>"
    COMMENT "Copying binary to NuGet directory"
)

# Create NuGet package using modern dotnet pack
add_custom_command(TARGET nuget
    POST_BUILD
    COMMAND "${DOTNET_EXECUTABLE}" pack "${CMAKE_SOURCE_DIR}/RibosoftAlgo.csproj" 
        --configuration Release
        --output "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}"
        --verbosity normal
        -p:PackagePlatform=${CMAKE_SYSTEM_NAME_LOWER}
        -p:OutputPath="${NUGET_PACKAGE_OUTPUT_DIR}/"
    COMMENT "Creating NuGet package with dotnet pack"
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
)

# Clear dotnet nuget cache
add_custom_command(TARGET nuget
    POST_BUILD
    COMMAND "${DOTNET_EXECUTABLE}" nuget locals all --clear
    COMMENT "Clearing dotnet nuget cache"
)