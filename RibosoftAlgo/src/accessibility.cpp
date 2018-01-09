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

#define MAX_TEMPLATE_LENGTH         200
#define EXTENDED_CUTSITE_LENGTH     100

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

        if (strlen(substrateTemplate) > MAX_TEMPLATE_LENGTH) {
            return R_APPLICATION_ERROR::R_INVALID_TEMPLATE_LENGTH;
        }

        idx_t subSequenceStart = cutsiteIndex - EXTENDED_CUTSITE_LENGTH;
        idx_t subSequenceEnd = cutsiteIndex + EXTENDED_CUTSITE_LENGTH;

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
        char* subSequence = new char[length + 1];
        strncpy(subSequence, &substrateSequence[subSequenceStart], length);
        subSequence[length] = '\0';

        char* constraints = new char[length + 1];
        memset(constraints, '.', length);

        // Set constraints for not folding here
        idx_t constraintStart = cutsiteIndex - subSequenceStart - cutsiteNumber;
        idx_t constraintEnd = constraintStart + (idx_t) strlen(substrateTemplate);

        if (constraintStart < 0) {
            constraintStart = 0;
        }

        if (constraintEnd > length) {
            constraintEnd = (idx_t) length - 1;
        }

        for (idx_t i = constraintStart; i < constraintEnd; ++i) {
            constraints[i] = 'x';
        }

        constraints[length] = '\0';

        // Default fold
        char* defaultMFEStructure = new char[length + 1];
        vrna_fold_compound_t* defaultFoldCompound = vrna_fold_compound(subSequence, NULL, VRNA_OPTION_DEFAULT);
        float defaultMFE = (float) vrna_mfe(defaultFoldCompound, defaultMFEStructure);

        // Fold with constraints
        char* constraintMFEStructure = new char[length + 1];
        unsigned int constraint_options = VRNA_CONSTRAINT_DB_DEFAULT;
        vrna_fold_compound_t* constraintFoldCompound = vrna_fold_compound(subSequence, NULL, VRNA_OPTION_DEFAULT);
        vrna_constraints_add(constraintFoldCompound, (const char *) constraints, constraint_options);
        float constraintMFE = (float) vrna_mfe(constraintFoldCompound, constraintMFEStructure);

        // Get absolute value of delta(MFE)
        delta = std::fabs(constraintMFE - defaultMFE);

        // Free memory
        delete[] subSequence;
        delete[] constraints;
        delete[] defaultMFEStructure;
        delete[] constraintMFEStructure;

        vrna_fold_compound_free(defaultFoldCompound);
        vrna_fold_compound_free(constraintFoldCompound);

        return R_SUCCESS::R_STATUS_OK;
    }
}

RIBOSOFT_NAMESPACE_END