#include "dll.h"

#include <cstdlib>
#include <cstring>
#include <cmath>
#include <regex>

#include "functions.h"

//! \namespace ribosoft
namespace ribosoft {

/*!
 * \brief Accessibility score.
 * Used to calculate the accessibility of the cutsite in the RNA sequence.
 * ViennaRNA library used to fold the RNA sequence w/o constraints.
 * Score is evaluated as perfect (0) if the binding arms are not paired on the RNA,
 * or the annealing temperature of the binding arms.
 *
 * Understanding return values:
 * - R_INVALID_NUCLEOTIDE | rna has an invalid nucleotide
 * - R_STRUCT_LENGTH_DIFFER | sequence and structure lengths do not match
 * - R_VIENNA_RNA_ERROR | An error has occured with ViennaRNA. Contact us with details.
 *
 ***************************************************************************
 * \param substrateSequence substrate sequence from candidate
 * \param substrareStructure substrate structure from the candidate
 * \param foldedStructure structure of target sequence on rna (folded using ViennaRNA)
 * \param na_concentration Sodium (Na+) concentration (in moles)
 * \param probe_concentration Nucleic acid concentration in excess (in moles)
 * \param score Out variable for accessibility score
 * \return Status Code
 */
DLL_PUBLIC R_STATUS accessibility(const char* substrateSequence, const char* substrateStructure, const char* foldedStructure, const float na_concentration, const float probe_concentration, /*out*/ float& score)
{
    R_STATUS status;

    // validate input sequence
    status = validate_sequence(substrateSequence);
    if (status != R_SUCCESS::R_STATUS_OK) {
        return status;
    }

    std::string substrate_sequence = substrateSequence;
    std::string substrate_structure = substrateStructure;
    std::string folded_structure = foldedStructure;

    if (substrate_sequence.length() != substrate_structure.length() ||
        substrate_structure.length() != folded_structure.length()) {
        return R_APPLICATION_ERROR::R_STRUCT_LENGTH_DIFFER;
    }

    // TODO: minimum chosen arbitrarily; will change once we have more science info
    if (na_concentration < 0.0000000001f) {
        return R_APPLICATION_ERROR::R_INVALID_CONCENTRATION;
    }

    if (probe_concentration < 0.0000000001f) {
        return R_APPLICATION_ERROR::R_INVALID_CONCENTRATION;
    }

    std::regex base_regex("[0-9a-zA-Z]+");

    bool isSingleStranded = true;

    for (std::sregex_iterator i = std::sregex_iterator(substrate_structure.begin(), substrate_structure.end(), base_regex);
        i != std::sregex_iterator();
        ++i)
    {
        std::smatch match = *i;
        for (int j = 0; j < match.length(); j++)
        {
            if (folded_structure[match.position() + j] != '.')
            {
                isSingleStranded = false;
                break;
            }
        }
    }

    if (isSingleStranded)
    {
        score = 0.0f;
        return R_SUCCESS::R_STATUS_OK;
    }
    else
    {
        return anneal(substrateSequence, substrateStructure, na_concentration, probe_concentration, score);
    }
}

}