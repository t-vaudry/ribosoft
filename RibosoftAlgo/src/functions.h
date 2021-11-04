#pragma once

#include "dll.h"
#include "error.h"

#include <cstdint>
#include <cstring>
#include <string>

//! \namespace ribosoft
namespace ribosoft {

typedef uint8_t idx_t; //!< Index value

#pragma pack(push, 8)
/*! \struct fold_output
 * \brief Structure for the complex output type for the folding algorithm
 */
struct fold_output {
    char* structure; //!< Secondary structure from folding
    float probability; //!< Probability of structure in the free energy distribution
};
#pragma pack(pop)

/*! \fn validate_sequence
 * \brief validate_sequence
 * Validation function used to confirm that sequence contains only base nucleotides (A,C,G,U)
 * @file validation.cpp
 */
extern "C" DLL_PUBLIC R_STATUS validate_sequence(const char* sequence);

/*! \fn validate_structure
 * \brief validate_structure
 * Validation function used to confirm that structure has proper bonds
 * @file validation.cpp
 */
extern "C" DLL_PUBLIC R_STATUS validate_structure(const char* structure);

/*! \fn accessibility
 * \brief accessibility
 * Accessibility of cutsite on the substrate
 * @file accessibility.cpp
 */
extern "C" DLL_PUBLIC R_STATUS accessibility(const char* substrate_sequence, const char* substrate_structure, const char* folded_structure, const float na_concentration, const float probe_concentration, const float target_temp, /*out*/ float& score);


/*! \fn anneal
 * \brief anneal
 * Annealing temperature of binding regions for ribozyme
 * @file anneal.cpp
 */
extern "C" DLL_PUBLIC R_STATUS anneal(const char* sequence, const char* structure, const float na_concentration, const float probe_concentration, const float target_temp, float& temp);

/*! \fn fold
 * \brief fold
 * Fold function used to fold sequence with ViennaRNA
 * @file fold.cpp
 */
extern "C" DLL_PUBLIC R_STATUS fold(const char* sequence, const float env_temp, /*out*/ fold_output*& output, /*out*/ size_t& size);

/*! \fn fold_output_free
 * \brief fold_output_free
 * Function to free fold structure memory
 * @file fold.cpp
 */
extern "C" DLL_PUBLIC void fold_output_free(fold_output* output, size_t size);

/*! \fn mfe_default_fold
 * \brief mfe_deafult_fold
 * Fold function used to fold sequence w/o constraints with ViennaRNA
 * @file mfe_default_fold.cpp
 */
extern "C" DLL_PUBLIC R_STATUS mfe_default_fold(const char* sequence, const float env_temp, /*out*/ char*& structure);

/*! \fn fold_output_free
 * \brief fold_output_free
 * Function to free fold structure memory
 * @file fold.cpp
 */
extern "C" DLL_PUBLIC void mfe_default_fold_free(char* output);

/*! \fn structure
 * \brief structure
 * Comparison of secondary structures
 * @file structure.cpp
 */
extern "C" DLL_PUBLIC R_STATUS structure(const char* candidate, const char* ideal, /*out*/ float& distance);

}
