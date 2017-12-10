#include <catch.hpp>

#include <functions.h>

TEST_CASE("equal structures", "[structure]") {
    float dist;
    structure("..", "..", dist);
    REQUIRE(dist == 0.0f);

    structure("()", "()", dist);
    REQUIRE(dist == 0.0f);
}

TEST_CASE("not equal structures", "[structure]") {
    float dist;
    structure("..()..", "((..))", dist);
    REQUIRE(dist == 8.0f);

    structure("(.().)()", "...()().", dist);
    REQUIRE(dist == 6.0f);
}

TEST_CASE("not equal length structures", "[structure]") {
    float dist;
    structure("..()..", "((.))", dist);
    REQUIRE(dist == -1.0f);

    structure("(.().)()", ".()().", dist);
    REQUIRE(dist == -1.0f);
}