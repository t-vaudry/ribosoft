#include "dll.h"

#include <cstring>
#include <regex>
#include <stack>

#include "functions.h"

//! \namespace ribosoft
namespace ribosoft {

#define EMPTY(str) if (strlen(str) == 0) return R_APPLICATION_ERROR::R_EMPTY_PARAMETER; //!< Macro to determine if parameter is empty

/*!
 * \brief Sequence Validation
 * Used to determine if sequence only contains base nucleotides (A,C,G,U)
 *
 * Understanding return values:
 * - R_INVALID_NUCLEOTIDE | Invalid nucleotide found in sequence
 *************************************************************************
 *
 * @param sequence Sequence to be validated
 * @return Status Code
 */
R_STATUS validate_sequence(const char* sequence)
{
    EMPTY(sequence);

    std::regex nucleotides("[^ACGU]+");
    if (std::regex_search(sequence, nucleotides)) {
        return R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE;
    }

    return R_SUCCESS::R_STATUS_OK;
}

/*!
 * \brief Used to determine if structure has proper bonds
 *
 * Understanding return values:
 * - R_INVALID_STRUCT_ELEMENT | Element in structure is invalid
 * - R_BAD_PAIR_MATCH | Bonds are not perfect in structure
 ***************************************************************
 *
 * @param structure Structure to be validated
 * @return Status Code
 */
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

}