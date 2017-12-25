#include <catch.hpp>

#include <functions.h>

#include <cmath>

float DELTA = 0.05;

TEST_CASE("yG, zC (numerator) of 0; sequence length of 1", "[anneal]") {
	const char* sequence = "A";
	const char* structure = "0";
	float na_concentration = 1.0;
	float temp = anneal(sequence, structure, na_concentration);
	REQUIRE(fabs(temp-(-740.2)) < DELTA);
}

TEST_CASE("simple sequence and structure", "[anneal]") {
	const char* sequence = "AAUUUCCCCGGGGG";
	const char* structure = "0123abxyzABXYZ";
	float na_concentration = 1.0;
	float temp = anneal(sequence, structure, na_concentration);
	REQUIRE(fabs(temp-73.9429) < DELTA);
}

TEST_CASE("invalid (0) concentration", "[anneal]") {
	const char* sequence = "AAUUUCCCCGGGGG";
	const char* structure = "0123abxyzABXYZ";
	float na_concentration = 0.0;
	float temp = anneal(sequence, structure, na_concentration);
	REQUIRE(temp == R_APPLICATION_ERROR::R_INVALID_CONCENTRATION);
}

TEST_CASE("invalid base", "[anneal]") {
	const char* sequence = "AAU_UCCCCGGGGG";
	const char* structure = "0123abxyzABXYZ";
	float na_concentration = 0.0;
	float temp = anneal(sequence, structure, na_concentration);
	REQUIRE(temp == R_APPLICATION_ERROR::R_INVALID_BASE);
}

TEST_CASE("mismatch between sequence and structure lengths", "[anneal]") {
	const char* sequence = "AAUUCCCCGG";
	const char* structure = "0123abxyzABXYZ";
	float na_concentration = 0.0;
	float temp = anneal(sequence, structure, na_concentration);
	REQUIRE(temp == R_APPLICATION_ERROR::R_SEQUENCE_STRUCTURE_MISMATCH);
}

TEST_CASE("structure and sequence including all ignored characters", "[anneal]") {
	const char* sequence = "AUCG ;:`[]~!@#$%^&*(){}/=\?+|-_AAAAAAAAAAAAAAAUUUUUUUUUUUUUUUCCCCCCCCCCCCCCCGGGGGGGGGGGGGGGAU";
	const char* structure = "azAZ ;:`[]~!@#$%^&*(){}/=\?+|-_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	float na_concentration = 1.0;
	float temp = anneal(sequence, structure, na_concentration);
	REQUIRE(fabs(temp-22.0516) < DELTA);
}