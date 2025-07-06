#include <catch2/catch_amalgamated.hpp>


#include "functions.h"

using namespace ribosoft;
using Catch::Approx;

TEST_CASE("equal structures", "[structure]") {
    float dist;
    R_STATUS status = structure("..", "..", dist);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(dist == 0.0f);

    status = structure("()", "()", dist);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(dist == 0.0f);
}

TEST_CASE("not equal structures", "[structure]") {
    float dist;
    R_STATUS status = structure("..()..", "((..))", dist);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(dist == 8.0f);

    status = structure("(.().)()", "...()().", dist);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(dist == 6.0f);
}

TEST_CASE("not equal length structures", "[structure]") {
    float dist;
    R_STATUS status = structure("..()..", "((.))", dist);
    REQUIRE(status == R_APPLICATION_ERROR::R_STRUCT_LENGTH_DIFFER);

    status = structure("(.().)()", ".()().", dist);
    REQUIRE(status == R_APPLICATION_ERROR::R_STRUCT_LENGTH_DIFFER);
}

TEST_CASE("invalid structures", "[structure]") {
    float dist;
    R_STATUS status = structure("wefwfwfsd", "", dist);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_STRUCT_ELEMENT);

    status = structure("..))", "..", dist);
    REQUIRE(status == R_APPLICATION_ERROR::R_BAD_PAIR_MATCH);

    status = structure("..()", "wefwfwfsdwrg", dist);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_STRUCT_ELEMENT);

    status = structure("..()()()", "(()()))", dist);
    REQUIRE(status == R_APPLICATION_ERROR::R_BAD_PAIR_MATCH);
}