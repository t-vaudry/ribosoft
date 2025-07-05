#include "dll.h"

#include <cstdlib>
#include <cstring>
#include <cmath>

#include <ViennaRNA/data_structures.h>
#include <ViennaRNA/constraints.h>

#include "functions.h"

extern "C"
{
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
     *  @see  vrna_fold_compound_free(), vrna_fold_compound_comparative(), VRNA_md_t, VRNA_OPTION_MFE,
     *        VRNA_OPTION_PF, VRNA_OPTION_EVAL_ONLY, VRNA_OPTION_WINDOW
     *
     *  @param    sequence    A single sequence, or two concatenated sequences seperated by an '&' character
     *  @param    md_p        An optional set of model details
     *  @param    options     The options for DP matrices memory allocation
     *  @return               A prefilled vrna_fold_compound_t that can be readily used for computations
     */
    vrna_fold_compound_t* vrna_fold_compound(const char* sequence, vrna_md_t* md_p, unsigned int options);

    /**
     *  @brief Compute minimum free energy and an appropriate secondary
     *  structure of an RNA sequence, or RNA sequence alignment
     *
     *  Depending on the type of the provided vrna_fold_compound_t, this function
     *  predicts the MFE for a single sequence, or a corresponding averaged MFE for
     *  a sequence alignment. If backtracking is activated, it also constructs the
     *  corresponding secondary structure, or consensus structure.
     *  Therefore, the second parameter, @a structure, has to point to an allocated
     *  block of memory with a size of at least @f$\mathrm{strlen}(\mathrm{sequence})+1@f$ to
     *  store the backtracked MFE structure. (For consensus structures, this is the length of
     *  the alignment + 1. If @p NULL is passed, no backtracking will be performed.
     *
     *  @ingroup mfe_fold
     *
     *  @note This function is polymorphic. It accepts vrna_fold_compound_t of type
     *        VRNA_FC_TYPE_SINGLE, and VRNA_FC_TYPE_COMPARATIVE.
     *
     *  @see vrna_fold_compound_t, vrna_fold_compound(), vrna_fold(), vrna_circfold(),
     *        vrna_fold_compound_comparative(), vrna_alifold(), vrna_circalifold()
     *
     *  @param vc             fold compound
     *  @param structure      A pointer to the character array where the
     *                        secondary structure in dot-bracket notation will be written to (Maybe NULL)
     *
     *  @return the minimum free energy (MFE) in kcal/mol
     */
    float vrna_mfe(vrna_fold_compound_t* vc, char* structure);
}

//! \namespace ribosoft
namespace ribosoft {
    /*!
     * \brief MFE default fold.
     * Used to calculate the accessibility of the cutsite in the RNA sequence.
     * ViennaRNA library used to fold the RNA sequence w/o constraints.
     *
     * Understanding return values:
     * - R_INVALID_NUCLEOTIDE | rna has an invalid nucleotide
     * - R_VIENNA_RNA_ERROR | An error has occured with ViennaRNA. Contact us with details.
     *
     ***************************************************************************
     * \param sequence to fold
     * \param delta Out string containing the structure of the input sequence
     * \return Status Code
     */
    DLL_PUBLIC R_STATUS mfe_default_fold(const char* sequence, /*out*/ char*& structure)
    {
        R_STATUS status = validate_sequence(sequence);
        if (status != R_SUCCESS::R_STATUS_OK) {
            return status;
        }

        int length = strlen(sequence);

        // Copy substring of sequence that will be folded
        char* local_sequence = new char[length + 1];
        strncpy(local_sequence, sequence, length);
        local_sequence[length] = '\0';

        // Default fold
        structure = new char[length + 1];
        vrna_fold_compound_t* defaultFoldCompound = vrna_fold_compound(local_sequence, NULL, VRNA_OPTION_DEFAULT);
        (void)vrna_mfe(defaultFoldCompound, structure); // MFE value not used, just computing structure

        structure[length] = '\0';

        // Free memory
        delete[] local_sequence;

        vrna_fold_compound_free(defaultFoldCompound);

        return R_SUCCESS::R_STATUS_OK;
    }

    /*!
    * \brief Free memory from default fold
    * Used to free the memory from the fold structure
    *
    ***************************************************************************************
    * @param output Fold structure to be freed
    */
    DLL_PUBLIC void mfe_default_fold_free(char* structure)
    {
        delete[] structure;
    }
}