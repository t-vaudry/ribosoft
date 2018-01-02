#include "dll.h"

#include <cstdlib>
#include <cstring>

#include <ViennaRNA/data_structures.h>
#include <ViennaRNA/subopt.h>

#include "functions.h"

extern "C" {
    vrna_fold_compound_t* vrna_fold_compound(const char*, vrna_md_t*, unsigned int);
}

RIBOSOFT_NAMESPACE_START

extern "C"
{
    DLL_PUBLIC R_STATUS fold(const char* sequence, /*out*/ fold_output*& output)
    {
        // validate input sequence
        R_STATUS status = validate_sequence(sequence);
        if (status != R_SUCCESS::R_STATUS_OK) {
            return status;
        }

        // get a vrna_fold_compound with default settings
        vrna_fold_compound_t *vc = vrna_fold_compound(sequence, NULL, VRNA_OPTION_DEFAULT);

        // fold with suboptimal structures
        // TODO: consider passing energy range from user input
        vrna_subopt_solution_t *sol = vrna_subopt(vc, 500, 1, NULL);

        // initialize output
        size_t solution_size = 0;
        while(sol[solution_size].structure) {
            solution_size++;
        }

        size_t length = strlen(sequence);
        output = new fold_output[solution_size];
        for (idx_t i = 0; i < solution_size; i++) {
            output[i].structure = new char[length + 1];
            strncpy(output[i].structure, sol[i].structure, length);
            output[i].structure[length] = '\0';
            output[i].energy = sol[i].energy;
        }

        // free memory
        vrna_fold_compound_free(vc);
        for (idx_t i = 0; i < solution_size; i++) {
            free(sol[i].structure);
        }
        free(sol);

        return R_SUCCESS::R_STATUS_OK;
    }
}

RIBOSOFT_NAMESPACE_END
