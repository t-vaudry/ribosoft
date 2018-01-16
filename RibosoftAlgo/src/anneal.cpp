#include "dll.h"

#include <cstring>
#include <regex>
#include <iterator>
#include <cmath>
#include <vector>

#include "functions.h"

RIBOSOFT_NAMESPACE_START

R_STATUS anneal(const char* sequence, const char* structure, const float na_concentration, float& temp)
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
    if (na_concentration < 0.0000000001) {
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

        int a_count = 0;
        int u_count = 0;
        int c_count = 0;
        int g_count = 0;
        float wA;
        float xU;
        float zC;
        float yG;

        std::string substring = substrings.at(i);
        size_t length = substring.size();

        for (int j = 0; j < length; j++) {
            char base = substring[j];
            switch (base) {
            case 'A':
                a_count++;
                break;
            case 'U':
                u_count++;
                break;
            case 'C':
                c_count++;
                break;
            case 'G':
                g_count++;
                break;
            default:
                // Error while counting bases: sequence contains non-base character (must be A, U, C, or G);
                return R_APPLICATION_ERROR::R_INVALID_NUCLEOTIDE;
            }
        }

        wA = float(a_count);
        xU = float(u_count);
        zC = float(c_count);
        yG = float(g_count);

        float log_concentration;
        try
        {
            log_concentration = log(na_concentration);
        }
        catch (const std::exception&)
        {
            // Error while calculating log: concentration may be 0 (concentration cannot be 0);
            return R_APPLICATION_ERROR::R_INVALID_CONCENTRATION;
        }

        // Tm= 79.8 + 18.5*log10([Na+]) + (58.4 * (yG+zC)/(wA+xU+yG+zC)) + (11.8 * ((yG+zC)/(wA+xU+yG+zC))2) - (820/(wA+xU+yG+zC))
        float Tm = 79.8f + 18.5f * log_concentration + (58.4f * (yG + zC) / (wA + xU + yG + zC)) + (11.8f * ((yG + zC) / (wA + xU + yG + zC)) * 2.0f) - (820.0f / (wA + xU + yG + zC));
        temp_sum += Tm;
    }

    temp = temp_sum;
    return status;
}

RIBOSOFT_NAMESPACE_END