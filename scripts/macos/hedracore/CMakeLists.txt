cmake_minimum_required(VERSION 3.18)
project(hedracore_macos C CXX)
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
get_filename_component(HEDRA_CORE_SOURCE "${CMAKE_CURRENT_LIST_DIR}/../../../hedra-core" ABSOLUTE)
add_subdirectory("${HEDRA_CORE_SOURCE}" core)

# Upstream assigns Intel instruction flags to every build. These translation
# units compile to empty objects on ARM; the NEON unit supplies the implementation.
if(CMAKE_OSX_ARCHITECTURES STREQUAL "arm64")
    foreach(isa sse2 sse41 avx2 avx512)
        set_source_files_properties("${HEDRA_CORE_SOURCE}/src/fastnoise-simd/FastNoiseSIMD_${isa}.cpp"
            DIRECTORY "${HEDRA_CORE_SOURCE}" PROPERTIES COMPILE_FLAGS "")
    endforeach()
endif()
