#include <catch.hpp>

#include <functions.h>

TEST_CASE("equal structures", "[structure]") {
    REQUIRE(structure("..", "..") == 0.0f);
    REQUIRE(structure("()", "()") == 0.0f);
}

TEST_CASE("not equal structures", "[structure]") {
    REQUIRE(structure("..()..", "((..))") == 8.0f);
    REQUIRE(structure("(.().)()", "...()().") == 6.0f);
}

TEST_CASE("not equal length structures", "[structure]") {
    REQUIRE(structure("..()..", "((.))") == -1.0f);
    REQUIRE(structure("(.().)()", ".()().") == -1.0f);
}