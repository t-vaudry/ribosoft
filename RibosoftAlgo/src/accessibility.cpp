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

    /**
     *  @brief  Add constraints to a vrna_fold_compound_t data structure
     *
     *  Use this function to add/update the hard/soft constraints
     *  The function allows for passing a string 'constraint' that can either be a
     *  filename that points to a constraints definition file or it may be a
     *  pseudo dot-bracket notation indicating hard constraints. For the latter, the
     *  user has to pass the VRNA_CONSTRAINT_DB option. Also, the
     *  user has to specify, which characters are allowed to be interpreted as
     *  constraints by passing the corresponding options via the third parameter.
     *
     *  @see      vrna_hc_init(), vrna_hc_add_up(), vrna_hc_add_up_batch(), vrna_hc_add_bp(),
     *            vrna_sc_init(), vrna_sc_set_up(), vrna_sc_set_bp(),
     *            vrna_sc_add_SHAPE_deigan(),  vrna_sc_add_SHAPE_zarringhalam(),
     *            vrna_hc_free(), vrna_sc_free(),
     *            VRNA_CONSTRAINT_DB, VRNA_CONSTRAINT_DB_DEFAULT, VRNA_CONSTRAINT_DB_PIPE,
     *            VRNA_CONSTRAINT_DB_DOT, VRNA_CONSTRAINT_DB_X, VRNA_CONSTRAINT_DB_ANG_BRACK,
     *            VRNA_CONSTRAINT_DB_RND_BRACK, VRNA_CONSTRAINT_DB_INTRAMOL,
     *            VRNA_CONSTRAINT_DB_INTERMOL, VRNA_CONSTRAINT_DB_GQUAD
     *
     *  @ingroup  constraints
     *
     *  The following is an example for adding hard constraints given in
     *  pseudo dot-bracket notation. Here, @p vc is the vrna_fold_compound_t object,
     *  @p structure is a char array with the hard constraint in dot-bracket notation,
     *  and @p enforceConstraints is a flag indicating whether or not constraints for
     *  base pairs should be enforced instead of just doing a removal of base pair that
     *  conflict with the constraint.
     *
     *  In constrat to the above, constraints may also be read from file:
     *
     *  @see  vrna_hc_add_from_db(), vrna_hc_add_up(), vrna_hc_add_up_batch()
     *        vrna_hc_add_bp_unspecific(), vrna_hc_add_bp()
     *
     *  @param  vc            The fold compound
     *  @param  constraint    A string with either the filename of the constraint definitions
     *                        or a pseudo dot-bracket notation of the hard constraint. May be NULL.
     *  @param  options       The option flags
     */
    void vrna_constraints_add(vrna_fold_compound_t* vc, const char* constraint, unsigned int options);
}

//! \namespace ribosoft
namespace ribosoft {

#define MAX_TEMPLATE_LENGTH         200 //!< Max length for substrateSequence
#define EXTENDED_CUTSITE_LENGTH     100 //!< Extension of cutsite to include in accessibility score

/*!
 * \brief Accessibility score.
 * Used to calculate the accessibility of the cutsite in the RNA sequence.
 * ViennaRNA library used to fold the RNA sequence w/ and w/o constraints,
 * and calculates their minimum free energies. Score is evaluated as the
 * delta between the energies.
 *
 * Understanding return values:
 * - R_INVALID_NUCLEOTIDE | rna has an invalid nucleotide
 * - R_OUT_OF_RANGE | cutsiteIndex or cutsiteNumber is out of range
 * - R_INVALID_TEMPLATE_LENGTH | substrateSequence is greater than MAX_TEMPLATE_LENGTH
 * - R_VIENNA_RNA_ERROR | An error has occured with ViennaRNA. Contact us with details.
 *
 ***************************************************************************
 * \param rna Input RNA sequence from Job
 * \param substrateSequence Cutsite sequence from Ribozyme
 * \param cutsiteIndex Index of cutsite on rna
 * \param cutsiteNumber Index of cutsite on substrateSequence
 * \param delta Out variable for accessibility score
 * \return Status Code
 */
DLL_PUBLIC R_STATUS accessibility(const char* rna, const char* substrateSequence, const int cutsiteIndex, const int cutsiteNumber, /*out*/ float& delta)
{
    R_STATUS status = validate_sequence(rna);
    if (status != R_SUCCESS::R_STATUS_OK) {
	return status;
    }

    if (cutsiteIndex > strlen(rna) || cutsiteIndex < 0) {
        return R_APPLICATION_ERROR::R_OUT_OF_RANGE;
    }

    if (cutsiteNumber > strlen(substrateSequence) || cutsiteNumber < 0) {
        return R_APPLICATION_ERROR::R_OUT_OF_RANGE;
    }

    if (strlen(substrateSequence) > MAX_TEMPLATE_LENGTH) {
        return R_APPLICATION_ERROR::R_INVALID_TEMPLATE_LENGTH;
    }

    int subSequenceStart = cutsiteIndex - EXTENDED_CUTSITE_LENGTH;
    int subSequenceEnd = cutsiteIndex + EXTENDED_CUTSITE_LENGTH;

    // If cutsiteIndex is close to the beginning
    if (subSequenceStart < 0) {
        subSequenceStart = 0;
    }

    // If cutsiteIndex is close to the end
    if (subSequenceEnd > strlen(rna)) {
        subSequenceEnd = (int) strlen(rna) - 1;
    }

    int length = subSequenceEnd - subSequenceStart;

    // Copy substring of sequence that will be folded
    char* subSequence = new char[length + 1];
    strncpy(subSequence, &rna[subSequenceStart], length);
    subSequence[length] = '\0';

    char* constraints = new char[length + 1];
    memset(constraints, '.', length);

    // Set constraints for not folding here
    int constraintStart = cutsiteIndex - subSequenceStart - cutsiteNumber;
    int constraintEnd = constraintStart + (int) strlen(substrateSequence);

    if (constraintStart < 0) {
        constraintStart = 0;
    }

    if (constraintEnd > length) {
        constraintEnd = length - 1;
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