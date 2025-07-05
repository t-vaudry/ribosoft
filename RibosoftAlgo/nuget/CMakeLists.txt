add_custom_target(nuget ALL DEPENDS RibosoftAlgo)

# Find dotnet executable
find_program(DOTNET_EXECUTABLE dotnet)

if (NOT DOTNET_EXECUTABLE)
    message(FATAL_ERROR "dotnet not found (install dotnet-sdk-8.0 or later)")
endif ()

# Determine platform for NuGet package
if(WIN32)
    set(NUGET_PLATFORM "windows")
elseif(APPLE)
    set(NUGET_PLATFORM "osx")
else()
    set(NUGET_PLATFORM "linux")
endif()

# Copy library to multiple locations to ensure NuGet can find it
add_custom_command(TARGET nuget
    POST_BUILD
    COMMAND "${CMAKE_COMMAND}" -E copy
        "$<TARGET_FILE:RibosoftAlgo>"
        "${NUGET_PACKAGE_OUTPUT_DIR}/$<TARGET_FILE_NAME:RibosoftAlgo>"
    COMMAND "${CMAKE_COMMAND}" -E copy
        "$<TARGET_FILE:RibosoftAlgo>"
        "${CMAKE_SOURCE_DIR}/$<TARGET_FILE_NAME:RibosoftAlgo>"
    COMMAND "${CMAKE_COMMAND}" -E copy
        "$<TARGET_FILE:RibosoftAlgo>"
        "${CMAKE_SOURCE_DIR}/build/$<TARGET_FILE_NAME:RibosoftAlgo>"
    COMMENT "Copying binary to NuGet directories"
)

# Create NuGet package using modern dotnet pack
add_custom_command(TARGET nuget
    POST_BUILD
    COMMAND "${DOTNET_EXECUTABLE}" pack "${CMAKE_SOURCE_DIR}/RibosoftAlgo.csproj" 
        --configuration Release
        --output "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}"
        --verbosity normal
        -p:PackagePlatform=${NUGET_PLATFORM}
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