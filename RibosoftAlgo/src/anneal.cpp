#include "dll.h"

#include <cstring>
#include <regex>
#include <iterator>
#include <cmath>
#include <vector>
#include <mutex>

#include "functions.h"

#include <melting.h>

//! \namespace ribosoft
namespace ribosoft {

std::mutex melting_mutex; //!< Mutex to lock access to MELTING library

/*!
 * \brief Annealing Temperature Score
 * Used to calculate the annealing temperature of the ribozyme to the
 * substrate. Using the MELTING library by Le Novère. MELTING, a free
 * tool to compute the melting temperature of nucleic acid duplex. 
 * Bioinformatics, 17: 1226-1227.
 *
 * Understanding return values:
 * - R_INVALID_NUCLEOTIDE | sequence has an invalid nucleotide
 * - R_STRUCT_LENGTH_DIFFER | sequence and structure lengths do not match
 * - R_INVALID_CONCENTRATION | na_concentration or probe_concentration are out of range
 * - R_INVALID_ARM_LENGTH | substring length of one of the arms is 1
 *
 ***************************************************************************************
 * \param sequence Substrate sequence
 * \param structure Substrate structure to determine binding regions
 * \param na_concentration Sodium (Na+) concentration (in moles)
 * \param probe_concentration Nucleic acid concentration in excess (in moles)
 * \param target_temp Target temperature of binding arms
 * \param temp Out variable for annealing temperature score
 * \return Status Code
 */
R_STATUS anneal(const char* sequence, const char* structure, const float na_concentration, const float probe_concentration, const float target_temp, float& temp)
{
    R_STATUS status;

    // validate input sequence
    status = validate_sequence(sequence);
    if (status != R_SUCCESS::R_STATUS_OK) {
        return status;
    }
    
    std::string local_sequence = sequence;
    std::string local_structure = structure;

    if (local_sequence.length() != local_structure.length()) {
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
    auto substruct_begin = std::sregex_iterator(local_structure.begin(), local_structure.end(), base_regex);
    auto substruct_end = std::sregex_iterator();

    size_t num_substrings = distance(substruct_begin, substruct_end);

    std::vector<std::string> substrings;

    for (std::sregex_iterator i = substruct_begin; i != substruct_end; ++i) {
        std::smatch match = *i;
        substrings.push_back(local_sequence.substr(match.position(), match.length()));
    }

    double temp_sum = 0.0;
    double difference = 0.0;

    for (int i = 0; i < substrings.size(); i++) {
        // A arm length of 1 will cause melting to crash
        // Ignore that arm
        if (substrings[i].length() != 1)
        {
            // Calculate melting temperature
            // a lock is needed as melting's melting is not threadsafe
            std::lock_guard<std::mutex> lock(melting_mutex);
           
            // Linear score until 4 degrees centigrade of difference
            // Exponential score after that 
            difference = fabs(melting(substrings[i].c_str(), na_concentration, probe_concentration) - target_temp);

            if (difference <= 4)
                temp_sum += difference;
            else
                temp_sum += pow(difference, 2);
        }
    }

    temp = static_cast<float>(temp_sum);
    return R_SUCCESS::R_STATUS_OK;
}

}