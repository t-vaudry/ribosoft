#include <catch.hpp>
#include <cstring>

#include "functions.h"

using namespace ribosoft;

TEST_CASE("default", "[fold]") {
    fold_output* output = nullptr;
    int size;
    R_STATUS status = fold("AUGUCUUAGGUGAUACGUGC", output, size);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(strcmp(output[0].structure, ".((((......)))).....") == 0);
    REQUIRE(output[0].energy == -1.60f);

    REQUIRE(strcmp(output[35].structure, "((((..(.....).))))..") == 0);
    REQUIRE(output[35].energy == 3.00f);
}

TEST_CASE("invalid sequence", "[fold]") {
    fold_output* output = nullptr;
    int size;
    R_STATUS status = fold("wfef", output, size);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
}
