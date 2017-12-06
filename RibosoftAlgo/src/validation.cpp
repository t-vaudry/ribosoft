#include "dll.h"

#include <string.h>
#include <regex>
#include <stack>

#include "functions.h"

RIBOSOFT_NAMESPACE_START

#define EMPTY(str) if (strlen(str) == 0) return R_APPLICATION_ERROR::R_EMPTY_PARAMETER;

R_STATUS validate_sequence(const char* sequence)
{
    EMPTY(sequence);

    std::regex nucleotides("[^ACGU]+");
    if (std::regex_search(sequence, nucleotides)) {
        return R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE;
    }

    return R_SUCCESS::R_STATUS_OK;
}

R_STATUS validate_structure(const char* structure)
{
    EMPTY(structure);

    std::regex structs("[^.(){}]+");
    if (std::regex_search(structure, structs)) {
        return R_APPLICATION_ERROR::R_INVALID_STRUCT_ELEMENT;
    }

    std::stack<idx_t> dblBonds;
    std::stack<idx_t> pseudoKnots;

    size_t len = strlen(structure);
    for (idx_t i = 0; i < len; ++i) {
        char element = structure[i];
        if (element == '(') {
            dblBonds.push(i);
        } else if (element == '{') {
            pseudoKnots.push(i);
        } else if (element == ')') {
            if (dblBonds.empty()) {
                return R_APPLICATION_ERROR::R_BAD_PAIR_MATCH;
            }
            dblBonds.pop();
        } else if (element == '}') {
            if (pseudoKnots.empty()) {
                return R_APPLICATION_ERROR::R_BAD_PAIR_MATCH;
            }
            pseudoKnots.pop();
        }
    }

    if (!dblBonds.empty()) {
        return R_APPLICATION_ERROR::R_BAD_PAIR_MATCH;
    }

    if (!pseudoKnots.empty()) {
        return R_APPLICATION_ERROR::R_BAD_PAIR_MATCH;
    }

    return R_SUCCESS::R_STATUS_OK;
}

RIBOSOFT_NAMESPACE_END