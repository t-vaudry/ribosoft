#include "dll.h"

#include <stdlib.h>
#include <string.h>

#include <ViennaRNA/RNAstruct.h>
#include <ViennaRNA/treedist.h>

#include "functions.h"

extern "C"
{
    DLL_PUBLIC void structure(const char* candidate, const char* ideal, float& distance)
    {
        // TODO: structure validation

        if (strlen(candidate) != strlen(ideal)) {
            // TODO: ERROR CODES!
            distance = -1.0f;
            return;
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
    }
}