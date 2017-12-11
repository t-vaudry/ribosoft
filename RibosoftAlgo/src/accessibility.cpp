#include "dll.h"

#include <stdlib.h>
#include <string.h>
#include <cmath>

#include <ViennaRNA\data_structures.h>
#include <ViennaRNA\constraints.h>

#include "functions.h"

extern "C"
{
    vrna_fold_compound_t* vrna_fold_compound(const char*, vrna_md_t*, unsigned int);
    float vrna_mfe(vrna_fold_compound_t*, char*);
    void vrna_constraints_add(vrna_fold_compound_t*, const char*, unsigned int);

    DLL_PUBLIC void accessibility(const char* substrateSequence, const char* substrateTemplate, const int cutsiteIndex, const int cutsiteNumber, float& delta)
    {
        // TODO: sequence validation

        if (cutsiteIndex > strlen(substrateSequence) || cutsiteIndex < 0) {
            // TODO: error code
            return;
        }

        if (cutsiteNumber > strlen(substrateTemplate) || cutsiteNumber < 0) {
            // TODO: error code
            return;
        }

        if (strlen(substrateTemplate) > 200) {
            // TODO: error code
            return;
        }

        // TODO: switch to idx_t
        int subSequenceStart = cutsiteIndex - 100;
        int subSequenceEnd = cutsiteIndex + 100;

        // If cutsiteIndex is close to the beginning
        if (subSequenceStart < 0) {
            subSequenceStart = 0;
        }

        // If cutsiteIndex is close to the end
        if (subSequenceEnd > strlen(substrateSequence)) {
            subSequenceEnd = (int) strlen(substrateSequence) - 1;
        }

        int length = subSequenceEnd - subSequenceStart;

        // Copy substring of sequence that will be folded
        char* subSequence = (char*) malloc(sizeof(char) * length);
        strncpy(subSequence, &substrateSequence[subSequenceStart], length);
        subSequence[length - 1] = '\0';

        char* constraints = (char*) malloc(sizeof(char) * length);
        memset(constraints, '.', length);

        // TODO: switch to idx_t
        // Set constraints for not folding here
        int constraintStart = cutsiteIndex - subSequenceStart - cutsiteNumber;
        int constraintEnd = constraintStart + (int) strlen(substrateTemplate);

        if (constraintStart < 0) {
            constraintStart = 0;
        }

        if (constraintEnd > strlen(subSequence)) {
            constraintEnd = (int) strlen(subSequence) - 1;
        }

        for (int i = constraintStart; i < constraintEnd; ++i) {
            constraints[i] = 'x';
        }

        constraints[length - 1] = '\0';

        // Default fold
        char* defaultMFEStructure = (char*) malloc(sizeof(char) * (strlen(subSequence) + 1));
        vrna_fold_compound_t* defaultFoldCompound = vrna_fold_compound(subSequence, NULL, VRNA_OPTION_DEFAULT);
        float defaultMFE = (float) vrna_mfe(defaultFoldCompound, defaultMFEStructure);

        // Fold with constraints
        char* constraintMFEStructure = (char*) malloc(sizeof(char) * (strlen(subSequence) + 1));
        unsigned int constraint_options = VRNA_CONSTRAINT_DB_DEFAULT;
        vrna_fold_compound_t* constraintFoldCompound = vrna_fold_compound(subSequence, NULL, VRNA_OPTION_DEFAULT);
        vrna_constraints_add(constraintFoldCompound, (const char *) constraints, constraint_options);
        float constraintMFE = (float)vrna_mfe(constraintFoldCompound, constraintMFEStructure);

        // Get absolute value of delta(MFE)
        delta = std::abs(constraintMFE - defaultMFE);
    }
}