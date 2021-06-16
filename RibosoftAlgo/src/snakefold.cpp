#include "dll.h"

#include <cstdlib>
#include <cmath>

#include <ViennaRNA/data_structures.h>
#include <ViennaRNA/subopt.h>
#include <ViennaRNA/part_func.h>

#include "functions.h"

extern "C" {
    /**
     *  @brief  Retrieve a vrna_fold_compound_t data structure for single sequences and hybridizing sequences
     *
     *  This function provides an easy interface to obtain a prefilled vrna_fold_compound_t by passing a single
     *  sequence, or two contatenated sequences as input. For the latter, sequences need to be seperated by
     *  an '&' character like this: @verbatim char *sequence = "GGGG&CCCC"; @endverbatim
     *
     *  The optional parameter @p md_p can be used to specify the model details for successive computations
     *  based on the content of the generated vrna_fold_compound_t. Passing NULL will instruct the function
     *  to use default model details.
     *  The third parameter @p options may be used to specify dynamic programming (DP) matrix requirements.
     *  Use the macros:
     *
     *  - VRNA_OPTION_MFE
     *  - VRNA_OPTION_PF
     *  - VRNA_OPTION_WINDOW
     *  - VRNA_OPTION_EVAL_ONLY
     *  - VRNA_OPTION_DEFAULT
     *
     *  to specify the required type of computations that will be performed with the vrna_fold_compound_t.
     *
     *  If you just need the folding compound serving as a container for your data, you can simply pass
     *  VRNA_OPTION_DEFAULT to the @p option parameter. This creates a vrna_fold_compound_t without DP
     *  matrices, thus saving memory. Subsequent calls of any structure prediction function will then take
     *  care of allocating the memory required for the DP matrices.
     *  If you only intend to evaluate structures instead of actually predicting them, you may use the
     *  VRNA_OPTION_EVAL_ONLY macro. This will seriously speedup the creation of the vrna_fold_compound_t.
     *
     *  @note The sequence string must be uppercase, and should contain only RNA (resp. DNA) alphabet depending
     *        on what energy parameter set is used
     *
     *  @see  vrna_fold_compound_free(), vrna_fold_compound_comparative(), vrna_md_t, VRNA_OPTION_MFE,
     *        VRNA_OPTION_PF, VRNA_OPTION_EVAL_ONLY, VRNA_OPTION_WINDOW
     *
     *  @param    sequence    A single sequence, or two concatenated sequences seperated by an '&' character
     *  @param    md_p        An optional set of model details
     *  @param    options     The options for DP matrices memory allocation
     *  @return               A prefilled vrna_fold_compound_t that can be readily used for computations
     */
    vrna_fold_compound_t* vrna_fold_compound(const char* sequence, vrna_md_t* md_p, unsigned int options);
}

//! \namespace ribosoft
namespace ribosoft {

#define EPSILON 0.000001 //!< Epsilon to determine if equal to zero

/*!
 * \brief SnakeFold
 * Used to fold the RNA sequence and provide the probabilities
 * of each fold in the distribution. Folding is done using ViennaRNA
 *
 * Understanding return values:
 * - R_INVALID_NUCLEOTIDE | sequence has an invalid nucleotide
 * - R_VIENNA_RNA_ERROR | Error from ViennaRNA, contact us with more details.
 *
 ***************************************************************************************
 * \param sequence Ribozyme sequence
 * \param output Out variable for fold structures
 * \param size Out variable for the size of the fold_output
 * \return Status Code
 */
DLL_PUBLIC R_STATUS snakefold(const char* sequence, /*out*/ fold_output*& output, /*out*/ size_t& size, int startOfSnakeSequence, int snakeLength)
{
    // validate input sequence
    R_STATUS status = validate_sequence(sequence);
    if (status != R_SUCCESS::R_STATUS_OK) {
        return status;
    }

    size_t length = strlen(sequence);

    char* constraints = new char[length + 1];
    memset(constraints, '.', length);

    for (idx_t i = startOfSnakeSequence; i < startOfSnakeSequence + snakeLength; ++i) {
        constraints[i] = 'x';
    }

    constraints[length] = '\0';

    // get a vrna_fold_compound with default settings
    vrna_fold_compound_t *vc = vrna_fold_compound(sequence, NULL, VRNA_OPTION_DEFAULT);
    unsigned int constraint_options = VRNA_CONSTRAINT_DB_DEFAULT;
    vrna_constraints_add(vc, (const char*)constraints, constraint_options);

    // fold with suboptimal structures
    // TODO: consider passing energy range from user input
    vrna_subopt_solution_t *sol = vrna_subopt(vc, 500, 1, NULL); //This gives access to the structures and other values, unlike accessibility.cpp

    // Get pf energy
    char *pf_struc = new char[length + 1];
    float energy = vrna_pf(vc, pf_struc);

    if (vc->exp_params == NULL) {
        return R_SYSTEM_ERROR::R_VIENNA_RNA_ERROR;
    }

    double kT = vc->exp_params->kT / 1000.;
    if (std::abs(kT) < EPSILON) {
        return R_SYSTEM_ERROR::R_VIENNA_RNA_ERROR;
    }

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
        output[i].probability = std::exp((energy - sol[i].energy) / kT);

        free(sol[i].structure);
    }

    // free memory
    free(sol);
    vrna_fold_compound_free(vc);
    free(pf_struc);

    return R_SUCCESS::R_STATUS_OK;
}

}
