#include <catch.hpp>
#include <cmath>

#include "functions.h"

using namespace ribosoft;

float DELTA = 0.05;

TEST_CASE("yG, zC (numerator) of 0; sequence length of 1", "[anneal]") {
    const char* sequence = "A";
    const char* structure = "0";
    const float na_concentration = 1.0;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, temp);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(fabs(temp-(-740.2)) < DELTA);
}

TEST_CASE("simple sequence and structure", "[anneal]") {
    const char* sequence = "AAUUUCCCCGGGGG";
    const char* structure = "0123abxyzABXYZ";
    const float na_concentration = 1.0;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, temp);
    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(fabs(temp-73.9429) < DELTA);
}

TEST_CASE("invalid (0) concentration", "[anneal]") {
    const char* sequence = "AAUUUCCCCGGGGG";
    const char* structure = "0123abxyzABXYZ";
    const float na_concentration = (float)0;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, temp);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_CONCENTRATION);
}

TEST_CASE("invalid (very small) concentration", "[anneal]") {
    const char* sequence = "AAUUUCCCCGGGGG";
    const char* structure = "0123abxyzABXYZ";
    const float na_concentration = 0.00000000000000001;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, temp);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_CONCENTRATION);
}

TEST_CASE("invalid base", "[anneal]") {
    const char* sequence = "AAU_UCCCCGGGGG";
    const char* structure = "0123ABXYZABXYZ";
    const float na_concentration = 1.0;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, temp);
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
}

TEST_CASE("mismatch between sequence and structure lengths", "[anneal]") {
    const char* sequence = "AAUUCCCCGG";
    const char* structure = "0123ABXYZABXYZ";
    const float na_concentration = 1.0;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, temp);
    REQUIRE(status == R_APPLICATION_ERROR::R_STRUCT_LENGTH_DIFFER);
}