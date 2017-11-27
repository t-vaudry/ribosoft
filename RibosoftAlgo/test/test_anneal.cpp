#include <catch.hpp>

#include <functions.h>

float DELTA = 0.05000;

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

TEST_CASE("structure and sequence including all ignored characters", "[anneal]") {
	const char* sequence = "AUCG ;:`[]~!@#$%^&*(){}/=\?+|-_AAAAAAAAAAAAAAAUUUUUUUUUUUUUUUCCCCCCCCCCCCCCCGGGGGGGGGGGGGGGAU";
	const char* structure = "azAZ ;:`[]~!@#$%^&*(){}/=\?+|-_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	float na_concentration = 1.0;
	float temp = anneal(sequence, structure, na_concentration);
	REQUIRE(fabs(temp-22.0516) < DELTA);
}