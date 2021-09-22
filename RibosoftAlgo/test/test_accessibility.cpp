#include <catch.hpp>
#include <cmath>

#include "functions.h"

#define DELTA 0.0001f

using namespace ribosoft;

/*
TEST_CASE("Successful accessibility", "[accessibility]") {
    float delta = -1.0f;
    R_STATUS status = accessibility("CAACUGCAUGUGAUGUGUAUGUGCUAAUCGGAUGCAUGUAGCUAGUGCGAUGCAACUGCAUGUGAUGUGUAUGUGCUAAUCGGAUGCAUGUAGCUAGUGCGUGCGGCGCGUAAUGCUAGUCGUAGUCGUAGUGCUAGUGUGCUGCUAGCUGUAGUGCUAUCGAUCGAUGCUAGCUGUAGUCGAUGCAACUGCAUGUGAUGUGUAUGUGCUAAUCGGAUGCAUGUAGCUAGUGCGUGCGGCGCGUAAUGCUAGUCGUAGUCGUAGUGCUAGUGUGCUGCUAGCUGUAGUGCUAUCGAUCGAUGCUAGCUGUAGUCGAAUGCGGCGCGUAAUGCUAGUCGUAGUCGUAGUGCUAGUGUGCUGCUAGCUGUAGUGCUAUCGAUCGAUGCUAGCUGUAGUCGA", "fed..234", 200, 4, delta);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(fabs(delta - 5.9f) < DELTA);
}

TEST_CASE("Test from actual data", "[accessibility]") {
    float delta = -1.0f;
    R_STATUS status = accessibility("CUUGAAGUGGUUUGUUGUGCUUGAAGAGACCCC","UUGUUGU",11,4,delta);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
}

TEST_CASE("Invalid substrate sequence", "[accessibility]") {
    float delta = -1.0f;
    R_STATUS status = accessibility("cjwdjvbq", "fed..234", 200, 4, delta);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
    REQUIRE(delta == -1.0f);
}

TEST_CASE("Invalid cutsiteIndex", "[accessibility]") {
    float delta = -1.0f;
    R_STATUS status = accessibility("AUGC", "fed..234", -5, 4, delta);
    REQUIRE(status == R_APPLICATION_ERROR::R_OUT_OF_RANGE);
    REQUIRE(delta == -1.0f);

    status = accessibility("AUGC", "fed..234", 800, 4, delta);
    REQUIRE(status == R_APPLICATION_ERROR::R_OUT_OF_RANGE);
    REQUIRE(delta == -1.0f);
}

TEST_CASE("Invalid cutsite", "[accessibility]") {
    float delta = -1.0f;
    R_STATUS status = accessibility("AUGC", "fed..234", 2, -4, delta);
    REQUIRE(status == R_APPLICATION_ERROR::R_OUT_OF_RANGE);
    REQUIRE(delta == -1.0f);

    status = accessibility("AUGC", "fed..234", 2, 20, delta);
    REQUIRE(status == R_APPLICATION_ERROR::R_OUT_OF_RANGE);
    REQUIRE(delta == -1.0f);
}

TEST_CASE("Invalid template", "[accessibility]") {
    float delta = -1.0f;
    R_STATUS status = accessibility("AUGC", "fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234fed..234", 2, 2, delta);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_TEMPLATE_LENGTH);
    REQUIRE(delta == -1.0f);
}

TEST_CASE("Invalid constraints", "[accessibility]") {
    float delta = -1.0f;
    R_STATUS status = accessibility("CUUGAAGUGGUUUGUUGUGCUUGAAGAGACCCC","UUGUUGUAUCUACUACCUAGUGAUGUCGGUAUGUGUGCUAUGUGAC",4,11,delta);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(delta == 5.6f);
}
*/