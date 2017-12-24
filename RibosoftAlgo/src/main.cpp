#include "dll.h"

#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <ViennaRNA/data_structures.h>

#include "functions.h"

RIBOSOFT_NAMESPACE_START

extern "C" {
    vrna_fold_compound_t* vrna_fold_compound(const char*, vrna_md_t*, unsigned int);
    float vrna_mfe(vrna_fold_compound_t*, char*);
}

extern "C"
{
    DLL_PUBLIC int math_add(int a, int b) {
        return a + b;
    }

    DLL_PUBLIC char* fold(const char* seq) {
        char  *mfe_structure = (char*) malloc(sizeof(char) * (strlen(seq) + 1));
        char  *prob_string   = (char*) malloc(sizeof(char) * (strlen(seq) + 1));

        /* get a vrna_fold_compound with default settings */
        vrna_fold_compound_t *vc = vrna_fold_compound(seq, NULL, VRNA_OPTION_DEFAULT);

        /* call MFE function */
        double mfe = (double)vrna_mfe(vc, mfe_structure);

        return mfe_structure;
    }
}

RIBOSOFT_NAMESPACE_END