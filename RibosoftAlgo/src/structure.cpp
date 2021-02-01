#include "dll.h"

#include <cstdlib>
#include <cstring>
#include <mutex>

#include <ViennaRNA/RNAstruct.h>
#include <ViennaRNA/treedist.h>

#include "functions.h"

//! \namespace ribosoft
namespace ribosoft {

std::mutex tree_edit_distance_mutex; //!< Mutex used to lock access to ViennaRNA library function `tree_edit_distance`

/*!
 * \brief Structure score
 * Used to calculate a comparison between two secondary structures, using ViennaRNA
 *
 * Understanding return values:
 * - R_BAD_PAIR_MATCH | Error in structure bonds
 * - R_STRUCT_LENGTH_DIFFER | candidate and ideal are different lengths
 * - R_VIENNA_RNA_ERROR | Error from ViennaRNA, contact us with more details
 ***********************************************************************************
 *
 * @param candidate Candidate secondary structure
 * @param ideal Ideal secondary structure
 * @param distance Out variable for structure score
 * @return State Code
 */
DLL_PUBLIC R_STATUS structure(const char* candidate, const char* ideal, /*out*/ float& distance)
{
    // Validate candidate structure
    R_STATUS status = validate_structure(candidate);
    if (status != R_SUCCESS::R_STATUS_OK) {
        return status;
    }

    // Validate ideal structure
    status = validate_structure(ideal);
    if (status != R_SUCCESS::R_STATUS_OK) {
        return status;
    }

    // Validate equal lengths
    if (strlen(candidate) != strlen(ideal)) {
        return R_APPLICATION_ERROR::R_STRUCT_LENGTH_DIFFER;
    }

    char* xstruc;
    Tree* T[2];

    // Make tree structure for candidate
    xstruc = expand_Full(candidate);
    T[0] = make_tree(xstruc);
    free(xstruc);

    // Make tree structure for ideal
    xstruc = expand_Full(ideal);
    T[1] = make_tree(xstruc);
    free(xstruc);

    // Calculate distance
    {
        // a lock is needed as vrna's tree_edit_distance is not threadsafe
        std::lock_guard<std::mutex> lock(tree_edit_distance_mutex);
        distance = tree_edit_distance(T[0], T[1]);
    }

    free_tree(T[0]);
    free_tree(T[1]);

    return R_SUCCESS::R_STATUS_OK;
}

}