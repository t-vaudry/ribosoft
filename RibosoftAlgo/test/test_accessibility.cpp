#include <catch.hpp>
#include <cmath>

#include "functions.h"

#define DELTA 0.0001f

using namespace ribosoft;

TEST_CASE("Perfect accessibility", "[accessibility]") {
    float score = -1.0f;
    R_STATUS status = accessibility("CAACUGCAUGUGAUG", "cba987654..3210", ".........()....", 1.0f, 0.5f, 22.0f, score);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(score == 0.0f);
}

TEST_CASE("Imperfect accesibility", "[accessibility]") {
    float score = -1.0f;
    R_STATUS status = accessibility("CAACUGCAUGUGAUG","cba987654..3210", "...((()((.)).).", 1.0f, 0.5f, 22.0f, score);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(fabs(score - 1939.23071f) < DELTA);
}

TEST_CASE("Invalid substrate sequence", "[accessibility]") {
    float score = -1.0f;
    R_STATUS status = accessibility("cjwdjvbq", "cba987654..3210", "...............", 1.0f, 0.5f, 22.0f, score);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
    REQUIRE(score == -1.0f);
}

TEST_CASE("Invalid structure length", "[accessibility]") {
    float score = -1.0f;
    R_STATUS status = accessibility("CAACUGCAUGUGAUG", "210", "...", 1.0f, 0.5f, 22.0f, score);
    REQUIRE(status == R_APPLICATION_ERROR::R_STRUCT_LENGTH_DIFFER);
    REQUIRE(score == -1.0f);
}
