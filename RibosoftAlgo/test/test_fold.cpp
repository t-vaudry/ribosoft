#include <catch2/catch_amalgamated.hpp>

#include <cstring>

#include "functions.h"

using namespace ribosoft;
using Catch::Approx;

TEST_CASE("default", "[fold]") {
    fold_output* output = nullptr;
    size_t size;
    R_STATUS status = fold("AUGUCUUAGGUGAUACGUGC", output, size);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(strcmp(output[0].structure, ".((((......)))).....") == 0);
    REQUIRE(output[0].probability == Approx(0.66907f).epsilon(0.01f));

    REQUIRE(strcmp(output[35].structure, "((((..(.....).))))..") == 0);
    REQUIRE(output[35].probability == Approx(0.00038f).epsilon(0.01f));

    REQUIRE(size == 51);

    float temp = 0.0f;
    for (int i = 0; i < size; i++) {
        temp += output[i].probability;
    }

    REQUIRE(temp == Approx(1.00f).epsilon(0.05f));
    fold_output_free(output, size);
}

TEST_CASE("valid", "[fold]") {
    fold_output* output = nullptr;
    size_t size;
    R_STATUS status = fold("AUUUUAGUGCUGAUGGCCAAUGCGCGAACCCAUCGGCGCUGUGA", output, size);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(strcmp(output[1].structure, ".((.((((((((((((.............)))))))))))).))") == 0);
    REQUIRE(output[1].probability == Approx(0.11585f).epsilon(0.01f));

    REQUIRE(strcmp(output[17].structure, ".....((((((((((((........)...)))))))))))....") == 0);
    REQUIRE(output[17].probability == Approx(0.00734f).epsilon(0.01f));

    REQUIRE(size == 173);

    float temp = 0.0f;
    for (int i = 0; i < size; i++) {
        temp += output[i].probability;
    }

    REQUIRE(temp == Approx(1.00f).epsilon(0.05f));
    fold_output_free(output, size);
}

TEST_CASE("invalid sequence", "[fold]") {
    fold_output* output = nullptr;
    size_t size;
    R_STATUS status = fold("wfef", output, size);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
}
