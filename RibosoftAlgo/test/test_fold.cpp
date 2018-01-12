#include <catch.hpp>
#include <cstring>

#include "functions.h"

using namespace ribosoft;

TEST_CASE("default", "[fold]") {
    fold_output* output = nullptr;
    size_t size;
    R_STATUS status = fold("AUGUCUUAGGUGAUACGUGC", output, size);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(strcmp(output[0].structure, ".((((......)))).....") == 0);
    REQUIRE(output[0].energy == -1.60f);

    REQUIRE(strcmp(output[35].structure, "((((..(.....).))))..") == 0);
    REQUIRE(output[35].energy == 3.00f);

    REQUIRE(size == 51);
}

TEST_CASE("valid", "[fold]") {
    fold_output* output = nullptr;
    size_t size;
    R_STATUS status = fold("AUUUUAGUGCUGAUGGCCAAUGCGCGAACCCAUCGGCGCUGUGA", output, size);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(strcmp(output[1].structure, ".((.((((((((((((.............)))))))))))).))") == 0);
    REQUIRE(output[1].energy == -16.20f);

    REQUIRE(strcmp(output[17].structure, ".....((((((((((((........)...)))))))))))....") == 0);
    REQUIRE(output[17].energy == -14.50f);

    REQUIRE(size == 173);
}

TEST_CASE("invalid sequence", "[fold]") {
    fold_output* output = nullptr;
    size_t size;
    R_STATUS status = fold("wfef", output, size);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
}
