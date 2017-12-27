#include "dll.h"

#include <stdlib.h>
#include <string.h>

#include <ViennaRNA/RNAstruct.h>
#include <ViennaRNA/treedist.h>

#include "functions.h"

RIBOSOFT_NAMESPACE_START

extern "C"
{
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
        distance = tree_edit_distance(T[0], T[1]);
        free_tree(T[0]);
        free_tree(T[1]);

        return R_SUCCESS::R_STATUS_OK;
    }
}

RIBOSOFT_NAMESPACE_END