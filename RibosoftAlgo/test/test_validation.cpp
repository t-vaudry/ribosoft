#include <catch.hpp>

#include "functions.h"

using namespace ribosoft;

TEST_CASE("Valid sequence", "[validate_sequence]") {
    REQUIRE(validate_sequence("AUGCGAUAGCUAUGUGCAUG") == R_SUCCESS::R_STATUS_OK);
    REQUIRE(validate_sequence("AAAUUUGCGCGAUAUCGGUC") == R_SUCCESS::R_STATUS_OK);
}

TEST_CASE("Invalid sequence", "[validate_sequence]") {
    REQUIRE(validate_sequence("ONEOFWBDASDJJWEFWF") == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
    REQUIRE(validate_sequence("&*(@#DEWFBIBIWUBEF") == R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE);
    REQUIRE(validate_sequence("") == R_APPLICATION_ERROR::R_EMPTY_PARAMETER);
}

TEST_CASE("Valid structure", "[validate_structure]") {
    REQUIRE(validate_structure("...()...") == R_SUCCESS::R_STATUS_OK);
    REQUIRE(validate_structure("(){}..") == R_SUCCESS::R_STATUS_OK);
    REQUIRE(validate_structure("{()()()...()()()}") == R_SUCCESS::R_STATUS_OK);
}

TEST_CASE("Invalid structure", "[validate_structure]") {
    REQUIRE(validate_structure("ONEOFWBDASDJJWEFWF") == R_APPLICATION_ERROR::R_INVALID_STRUCT_ELEMENT);
    REQUIRE(validate_structure("&*(@#DEWFBIBIWUBEF") == R_APPLICATION_ERROR::R_INVALID_STRUCT_ELEMENT);
    REQUIRE(validate_structure("") == R_APPLICATION_ERROR::R_EMPTY_PARAMETER);
    REQUIRE(validate_structure("..)(..") == R_APPLICATION_ERROR::R_BAD_PAIR_MATCH);
    REQUIRE(validate_structure("((())") == R_APPLICATION_ERROR::R_BAD_PAIR_MATCH);
    REQUIRE(validate_structure("(()){}{") == R_APPLICATION_ERROR::R_BAD_PAIR_MATCH);
    REQUIRE(validate_structure("}}{{()()") == R_APPLICATION_ERROR::R_BAD_PAIR_MATCH);
}