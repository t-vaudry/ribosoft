#include <catch.hpp>
#include <cmath>

#include "functions.h"

using namespace ribosoft;

float DELTA = 0.05f;

TEST_CASE("Simple check", "[anneal]") {
    const char* sequence = "AUGAUCGAUGCUGUAGCUGACU";
    const char* structure = "0000000000000000000000";
    const float na_concentration = 1.0f;
    const float probe_concentration = 0.05f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);

    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(fabs(temp - 4160.16f) < DELTA);
}

TEST_CASE("simple sequence and structure", "[anneal]") {
    const char* sequence = "AAUUUCCCCGGGGG";
    const char* structure = "0123abxyzABXYZ";
    const float na_concentration = 1.0f;
    const float probe_concentration = 0.05f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);

    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(fabs(temp - 4049.53f) < DELTA);
}

TEST_CASE("invalid (0) na concentration", "[anneal]") {
    const char* sequence = "AAUUUCCCCGGGGG";
    const char* structure = "0123abxyzABXYZ";
    const float na_concentration = 0.0f;
    const float probe_concentration = 0.05f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);

    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_CONCENTRATION);
}

TEST_CASE("invalid (0) probe concentration", "[anneal]") {
    const char* sequence = "AAUUUCCCCGGGGG";
    const char* structure = "0123abxyzABXYZ";
    const float na_concentration = 1.0f;
    const float probe_concentration = 0.0f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);

    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_CONCENTRATION);
}

TEST_CASE("invalid (very small) concentration", "[anneal]") {
    const char* sequence = "AAUUUCCCCGGGGG";
    const char* structure = "0123abxyzABXYZ";
    const float na_concentration = 0.00000000000000001f;
    const float probe_concentration = 0.05f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);
    
    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_CONCENTRATION);
}

TEST_CASE("invalid base", "[anneal]") {
    const char* sequence = "AAU_UCCCCGGGGG";
    const char* structure = "0123ABXYZABXYZ";
    const float na_concentration = 1.0f;
    const float probe_concentration = 0.05f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);

    REQUIRE(status == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
}

TEST_CASE("mismatch between sequence and structure lengths", "[anneal]") {
    const char* sequence = "AAUUCCCCGG";
    const char* structure = "0123ABXYZABXYZ";
    const float na_concentration = 1.0f;
    const float probe_concentration = 0.05f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);

    REQUIRE(status == R_APPLICATION_ERROR::R_STRUCT_LENGTH_DIFFER);
}

TEST_CASE("invalid arm length of one", "[anneal]") {
    const char* sequence = "A";
    const char* structure = "0";
    const float na_concentration = 1.0f;
    const float probe_concentration = 0.05f;
    const float target_temp = 22.0f;
    float temp;
    R_STATUS status = anneal(sequence, structure, na_concentration, probe_concentration, target_temp, temp);

    REQUIRE(status == R_SUCCESS::R_STATUS_OK);
    REQUIRE(temp == 0.0f);
}