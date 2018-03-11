#include "dll.h"

#include <cstdlib>

#include <ViennaRNA/data_structures.h>
#include <ViennaRNA/subopt.h>
#include <ViennaRNA/part_func.h>

#include "functions.h"

extern "C" {
    vrna_fold_compound_t* vrna_fold_compound(const char*, vrna_md_t*, unsigned int);
}

RIBOSOFT_NAMESPACE_START

extern "C"
{
    DLL_PUBLIC R_STATUS fold(const char* sequence, /*out*/ fold_output*& output, /*out*/ size_t& size)
    {
        // validate input sequence
        R_STATUS status = validate_sequence(sequence);
        if (status != R_SUCCESS::R_STATUS_OK) {
            return status;
        }

        size_t length = strlen(sequence);

        // get a vrna_fold_compound with default settings
        vrna_fold_compound_t *vc = vrna_fold_compound(sequence, NULL, VRNA_OPTION_DEFAULT);

        // fold with suboptimal structures
        // TODO: consider passing energy range from user input
        vrna_subopt_solution_t *sol = vrna_subopt(vc, 500, 1, NULL);

        // Get pf energy
        char *pf_struc = new char[length + 1];
        float energy = vrna_pf(vc, pf_struc);
        double kT = vc->exp_params->kT / 1000.;

        // initialize output
        size_t solution_size = 0;
        while(sol[solution_size].structure != nullptr) {
            solution_size++;
        }

        size = solution_size;
        output = new fold_output[solution_size];
        for (size_t i = 0; i < solution_size; ++i) {
            output[i].structure = new char[length + 1];
            strncpy(output[i].structure, sol[i].structure, length);
            output[i].structure[length] = '\0';
            output[i].probability = exp((energy - sol[i].energy) / kT);

            free(sol[i].structure);
        }

        // free memory
        free(sol);
        vrna_fold_compound_free(vc);
        free(pf_struc);

        return R_SUCCESS::R_STATUS_OK;
    }

	DLL_PUBLIC void fold_output_free(fold_output* output, size_t size)
	{
		if (output) {
			for (int i = 0; i < size; ++i) {
				delete[] output[i].structure;
				output[i].structure = nullptr;
			}

			delete[] output;
		}
	}
}

RIBOSOFT_NAMESPACE_END
