#pragma once

#include "dll.h"
#include "error.h"

#include <cstdint>
#include <cstring>

//! \namespace ribosoft
RIBOSOFT_NAMESPACE_START

typedef uint8_t idx_t; //!< Index value

#pragma pack(push, 8)
struct fold_output {
    char* structure; //!< Secondary structure from folding
    float probability; //!< Probability of structure in the free energy distribution
};
#pragma pack(pop)

/*! \func validate_sequence
 * Validation function used to confirm that sequence contains only base nucleotides (A,C,G,U)
 * @file validation.cpp
 */
extern "C" DLL_PUBLIC R_STATUS validate_sequence(const char* sequence);

/*! \func validate_structure
 * Validation function used to confirm that structure has proper bonds
 * @file validation.cpp
 */
extern "C" DLL_PUBLIC R_STATUS validate_structure(const char* structure);

/*! \func accessibility
 * Accessibility of cutsite on the substrate
 * @file accessibility.cpp
 */
extern "C" DLL_PUBLIC R_STATUS accessibility(const char* substrateSequence, const char* substrateTemplate, const int cutsiteIndex, const int cutsiteNumber, /*out*/ float& delta);

/*! \func anneal
 * Annealing temperature of binding regions for ribozyme
 * @file anneal.cpp
 */
extern "C" DLL_PUBLIC R_STATUS anneal(const char* sequence, const char* structure, const float na_concentration, const float probe_concentration, float& temp);

/*! \func fold
 * Fold function used to fold sequence with ViennaRNA
 * @file fold.cpp
 */
extern "C" DLL_PUBLIC R_STATUS fold(const char* sequence, /*out*/ fold_output*& output, /*out*/ size_t& size);

/*! \func fold_output_free
 * Function to free fold structure memory
 * @file fold.cpp
 */
extern "C" DLL_PUBLIC void fold_output_free(fold_output* output, size_t size);

/*! \func structure
 * Comparison of secondary structures
 * @file structure.cpp
 */
extern "C" DLL_PUBLIC R_STATUS structure(const char* candidate, const char* ideal, /*out*/ float& distance);

RIBOSOFT_NAMESPACE_END
