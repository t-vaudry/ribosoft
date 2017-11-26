#include "dll.h"

#include <stdlib.h>
#include <string.h>

#include <ViennaRNA/RNAstruct.h>
#include <ViennaRNA/treedist.h>

#include "functions.h"

extern "C"
{
    DLL_PUBLIC float structure(const char* candidate, const char* ideal)
    {
        // TODO: structure validation

        if (strlen(candidate) != strlen(ideal)) {
            // TODO: ERROR CODES!
            return -1.0f;
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
        float dist = tree_edit_distance(T[0], T[1]);
        free_tree(T[0]);
        free_tree(T[1]);

        return dist;
    }
}