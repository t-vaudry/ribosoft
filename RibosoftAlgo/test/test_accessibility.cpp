#include <catch.hpp>
#include <cmath>

#include "functions.h"

#define DELTA 0.0001f

TEST_CASE("Successful accessibility", "[accessibility]") {
    float delta = NULL;
    accessibility("CAACUGCAUGUGAUGUGUAUGUGCUAAUCGGAUGCAUGUAGCUAGUGCGAUGCAACUGCAUGUGAUGUGUAUGUGCUAAUCGGAUGCAUGUAGCUAGUGCGUGCGGCGCGUAAUGCUAGUCGUAGUCGUAGUGCUAGUGUGCUGCUAGCUGUAGUGCUAUCGAUCGAUGCUAGCUGUAGUCGAUGCAACUGCAUGUGAUGUGUAUGUGCUAAUCGGAUGCAUGUAGCUAGUGCGUGCGGCGCGUAAUGCUAGUCGUAGUCGUAGUGCUAGUGUGCUGCUAGCUGUAGUGCUAUCGAUCGAUGCUAGCUGUAGUCGAAUGCGGCGCGUAAUGCUAGUCGUAGUCGUAGUGCUAGUGUGCUGCUAGCUGUAGUGCUAUCGAUCGAUGCUAGCUGUAGUCGA", "fed..234", 200, 4, delta);
    REQUIRE(fabs(delta - 5.9f) < DELTA);
}

TEST_CASE("Invalid cutsiteIndex", "[accessibility]") {
    float delta = NULL;
    accessibility(".", "fed..234", -5, 4, delta);
    REQUIRE(delta == NULL);

    accessibility(".", "fed..234", 800, 4, delta);
    REQUIRE(delta == NULL);
}

TEST_CASE("Invalid cutsite", "[accessibility]") {
    float delta = NULL;
    accessibility(".", "fed..234", 200, -4, delta);
    REQUIRE(delta == NULL);

    accessibility(".", "fed..234", 200, 20, delta);
    REQUIRE(delta == NULL);
}

TEST_CASE("Invalid template", "[accessibility]") {
    float delta = NULL;
    accessibility(".", "fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234", 200, 4, delta);
    REQUIRE(delta == NULL);
}