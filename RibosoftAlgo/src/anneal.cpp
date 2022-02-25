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
    R_STATUS anneal(const char* sequence, const char* structure, const float na_concentration, const float probe_concentration, const float target_temp, const char* folded_structure, bool rna_anneal, float& temp)
    {
        R_STATUS status;

        // validate input sequence
        status = validate_sequence(sequence);
        if (status != R_SUCCESS::R_STATUS_OK) {
            return status;
        }

        std::string local_sequence = sequence;
        std::string local_structure = structure;
        std::string local_folded_structure = folded_structure;

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
        std::vector<std::string> folded_substrings;

        for (std::sregex_iterator i = substruct_begin; i != substruct_end; ++i) 
        {
            std::smatch match = *i;

            substrings.push_back(local_sequence.substr(match.position(), match.length()));

            if (rna_anneal)
            {
                folded_substrings.push_back(local_folded_structure.substr(match.position(), match.length()));
            }
        }

        if (rna_anneal)
        {
            double temp_sum = 0.0;
            //double difference = 0.0;
            double oligocalc_difference = 0.0;
            double melting_difference = 0.0;
            double melting_oligocalc_difference = 0.0;
            int numb_a = 0;
            int numb_c = 0;
            int numb_g = 0;
            int numb_u = 0;
            std::string current_substring_sequence;

            for (int i = 0; i < folded_substrings.size(); i++)
            {
                if (folded_substrings[i].length() > 0)
                {
                    std::lock_guard<std::mutex> lock(melting_mutex);

                    int current_position = 0;
                    int current_size = 0;

                    while (current_position + current_size < folded_substrings[i].length())
                    {
                        current_position += current_size;
                        current_size = 0;

                        while (folded_substrings[i].at(current_position) == '.')
                        {
                            current_position++;
                        
                            if (current_position >= folded_substrings[i].length())
                            {
                                break;
                            }
                        }
                        
                        if (current_position >= folded_substrings[i].length())
                        {
                            continue;
                        }
                        
                        while (folded_substrings[i].at(current_position + current_size) != '.')
                        {
                            current_size++;
                        
                            if (current_position + current_size >= folded_substrings[i].length())
                            {
                                break;
                            }
                        }
                        
                        current_substring_sequence = substrings[i].substr(current_position, current_size);

                        numb_a = std::count(current_substring_sequence.begin(), current_substring_sequence.end(), 'A');
                        numb_c = std::count(current_substring_sequence.begin(), current_substring_sequence.end(), 'C');
                        numb_g = std::count(current_substring_sequence.begin(), current_substring_sequence.end(), 'G');
                        numb_u = std::count(current_substring_sequence.begin(), current_substring_sequence.end(), 'U');
                        
                        if (current_substring_sequence.length() < 14)
                        {
                            oligocalc_difference = ((numb_a + numb_u) * 2) + ((numb_g + numb_c) * 4) - (16.6 * log10(0.05)) + (16.6 * log10(na_concentration / 1000));
                        }
                        else if (current_substring_sequence.length() < 51)
                        {
                            oligocalc_difference = 100.5 + (41 * (numb_g + numb_c) / (current_substring_sequence.length())) - (820 / current_substring_sequence.length()) + 16.6 * log10(na_concentration / 1000);
                        }
                        else
                        {
                            oligocalc_difference = 81.5 + (41 * (numb_g + numb_c) / (current_substring_sequence.length())) - (500 / current_substring_sequence.length()) + 16.6 * log10(na_concentration / 1000);
                        }

                        temp_sum += oligocalc_difference;
                    }
                }
            }

            temp = static_cast<float>(temp_sum);
            return R_SUCCESS::R_STATUS_OK;
        }
        else
        {
            double temp_sum = 0.0;
            //double difference = 0.0;
            double oligocalc_difference = 0.0;
            double melting_difference = 0.0;
            double melting_oligocalc_difference = 0.0;
            int numb_a = 0;
            int numb_c = 0;
            int numb_g = 0;
            int numb_u = 0;
            
            for (int i = 0; i < substrings.size(); i++) 
            {
                // A arm length of 1 will cause melting to crash
                // Ignore that arm
                if (substrings[i].length() > 0)
                {
                    // Calculate melting temperature
                    // a lock is needed as melting's melting is not threadsafe
                    std::lock_guard<std::mutex> lock(melting_mutex);
            
                    numb_a = std::count(substrings[i].begin(), substrings[i].end(), 'A');
                    numb_c = std::count(substrings[i].begin(), substrings[i].end(), 'C');
                    numb_g = std::count(substrings[i].begin(), substrings[i].end(), 'G');
                    numb_u = std::count(substrings[i].begin(), substrings[i].end(), 'U');
            
                    if (substrings[i].length() < 14)
                    {
                        oligocalc_difference = fabs(((numb_a + numb_u) * 2) + ((numb_g + numb_c) * 4) - (16.6 * log10(0.05)) + (16.6 * log10(na_concentration / 1000)) - target_temp);
                    }
                    else if (substrings[i].length() < 51)
                    {
                        oligocalc_difference = fabs(100.5 + (41 * (numb_g + numb_c) / (substrings[i].length())) - (820 / substrings[i].length()) + 16.6 * log10(na_concentration / 1000) - target_temp);
                    }
                    else
                    {
                        oligocalc_difference = fabs(81.5 + (41 * (numb_g + numb_c) / (substrings[i].length())) - (500 / substrings[i].length()) + 16.6 * log10(na_concentration / 1000) - target_temp);
                    }
            
                    if (oligocalc_difference <= 4)
                        temp_sum += oligocalc_difference;
                    else
                        temp_sum += pow(oligocalc_difference - 4, 2) + 4;
                }
            }
            
            temp = static_cast<float>(temp_sum);
            return R_SUCCESS::R_STATUS_OK;
        }
    }

}