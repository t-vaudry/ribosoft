#define CATCH_CONFIG_MAIN
#include <catch.hpp>

#include "functions.h"

using namespace ribosoft;

TEST_CASE("adding positive ints", "[math_add]") {
    REQUIRE(math_add(0, 0) == 0);
    REQUIRE(math_add(1, 2) == 3);
}

TEST_CASE("adding negative ints", "[math_add]") {
    REQUIRE(math_add(1, -1) == 0);
    REQUIRE(math_add(-1, -2) == -3);
}