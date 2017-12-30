#include "dll.h"

#include <cstdlib>
#include <cstring>
#include <cmath>

#include <ViennaRNA/data_structures.h>
#include <ViennaRNA/constraints.h>

#include "functions.h"

extern "C"
{
    vrna_fold_compound_t* vrna_fold_compound(const char*, vrna_md_t*, unsigned int);
    float vrna_mfe(vrna_fold_compound_t*, char*);
    void vrna_constraints_add(vrna_fold_compound_t*, const char*, unsigned int);
}

RIBOSOFT_NAMESPACE_START

extern "C"
{

    DLL_PUBLIC R_STATUS accessibility(const char* substrateSequence, const char* substrateTemplate, const int cutsiteIndex, const int cutsiteNumber, /*out*/ float& delta)
    {
        R_STATUS status = validate_sequence(substrateSequence);
        if (status != R_SUCCESS::R_STATUS_OK) {
        	return status;
        }

        if (cutsiteIndex > strlen(substrateSequence) || cutsiteIndex < 0) {
            return R_APPLICATION_ERROR::R_OUT_OF_RANGE;
        }

        if (cutsiteNumber > strlen(substrateTemplate) || cutsiteNumber < 0) {
            return R_APPLICATION_ERROR::R_OUT_OF_RANGE;
        }

        if (strlen(substrateTemplate) > 200) {
            return R_APPLICATION_ERROR::R_INVALID_TEMPLATE_LENGTH;
        }

        idx_t subSequenceStart = cutsiteIndex - 100;
        idx_t subSequenceEnd = cutsiteIndex + 100;

        // If cutsiteIndex is close to the beginning
        if (subSequenceStart < 0) {
            subSequenceStart = 0;
        }

        // If cutsiteIndex is close to the end
        if (subSequenceEnd > strlen(substrateSequence)) {
            subSequenceEnd = (idx_t) strlen(substrateSequence) - 1;
        }

        idx_t length = subSequenceEnd - subSequenceStart;

        // Copy substring of sequence that will be folded
        char* subSequence = (char*) malloc(sizeof(char) * length);
        strncpy(subSequence, &substrateSequence[subSequenceStart], length);
        subSequence[length - 1] = '\0';

        char* constraints = (char*) malloc(sizeof(char) * length);
        memset(constraints, '.', length);

        // Set constraints for not folding here
        idx_t constraintStart = cutsiteIndex - subSequenceStart - cutsiteNumber;
        idx_t constraintEnd = constraintStart + (idx_t) strlen(substrateTemplate);

        if (constraintStart < 0) {
            constraintStart = 0;
        }

        if (constraintEnd > strlen(subSequence)) {
            constraintEnd = (idx_t) strlen(subSequence) - 1;
        }

        for (idx_t i = constraintStart; i < constraintEnd; ++i) {
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
        float constraintMFE = (float) vrna_mfe(constraintFoldCompound, constraintMFEStructure);

        // Get absolute value of delta(MFE)
        delta = std::fabs(constraintMFE - defaultMFE);

        // Free memory
        free(subSequence);
        free(constraints);
        free(defaultMFEStructure);
        free(constraintMFEStructure);

        vrna_fold_compound_free(defaultFoldCompound);
        vrna_fold_compound_free(constraintFoldCompound);

        return R_SUCCESS::R_STATUS_OK;
    }
}

RIBOSOFT_NAMESPACE_END