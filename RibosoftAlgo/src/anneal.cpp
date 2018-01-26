#include "dll.h"

#include <cstring>
#include <regex>
#include <iterator>
#include <cmath>
#include <vector>

#include "functions.h"

#include <melting.h>

RIBOSOFT_NAMESPACE_START

R_STATUS anneal(const char* sequence, const char* structure, const float na_concentration, const float probe_concentration, float& temp)
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

    float temp_sum = 0.0;

    for (int i = 0; i < substrings.size(); i++) {

        temp_sum += (float) melting(substrings[i].c_str(), na_concentration, probe_concentration);
    }

    temp = temp_sum;
    return status;
}

RIBOSOFT_NAMESPACE_END