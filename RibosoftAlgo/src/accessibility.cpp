#include "dll.h"

#include <cstdlib>
#include <cstring>
#include <cmath>
#include <regex>
#include <vector>
#include <mutex>

#include "functions.h"

#include <melting.h>

//! \namespace ribosoft
namespace ribosoft {

extern std::mutex melting_mutex; //!< External reference to melting mutex

/*! \struct AccessibleSection
 * \brief Structure representing a contiguous accessible section within a binding arm
 */
struct AccessibleSection {
    int start_pos;      //!< Starting position within the arm
    int length;         //!< Length of accessible section
    std::string sequence; //!< Sequence of the accessible section
};

/*! \fn find_accessible_sections
 * \brief Find contiguous accessible sections within a binding arm
 * \param arm_sequence Sequence of the binding arm
 * \param arm_folded_structure Folded structure of the binding arm region
 * \return Vector of accessible sections
 */
std::vector<AccessibleSection> find_accessible_sections(const std::string& arm_sequence, const std::string& arm_folded_structure) {
    std::vector<AccessibleSection> sections;
    int current_start = -1;

    for (int i = 0; i < static_cast<int>(arm_sequence.length()); i++) {
        if (arm_folded_structure[i] == '.') {  // Accessible
            if (current_start == -1) {
                current_start = i;  // Start new section
            }
        } else {  // Bound/paired
            if (current_start != -1) {
                // End current section
                sections.push_back({
                    current_start,
                    i - current_start,
                    arm_sequence.substr(current_start, i - current_start)
                });
                current_start = -1;
            }
        }
    }
    
    // Handle section that goes to end
    if (current_start != -1) {
        sections.push_back({
            current_start,
            static_cast<int>(arm_sequence.length()) - current_start,
            arm_sequence.substr(current_start)
        });
    }

    return sections;
}

/*! \fn calculate_section_accessibility_score
 * \brief Calculate accessibility score for accessible sections within a binding arm
 * \param sections Vector of accessible sections
 * \param na_concentration Sodium concentration
 * \param probe_concentration Probe concentration
 * \param target_temp Target temperature
 * \return Accessibility score for the arm
 */
double calculate_section_accessibility_score(const std::vector<AccessibleSection>& sections,
                                           float na_concentration, float probe_concentration, float target_temp) {
    const int MIN_BINDING_LENGTH = 3;  // Minimum length for effective binding
    double total_score = 0.0;

    for (const auto& section : sections) {
        if (section.length < MIN_BINDING_LENGTH) {
            continue;  // Too short to be useful for binding
        }

        // Calculate melting temperature for this accessible section
        // Lock needed as melting library is not thread-safe
        std::lock_guard<std::mutex> lock(melting_mutex);
        double section_melting_temp = melting(section.sequence.c_str(), na_concentration, probe_concentration);

        // Calculate temperature difference penalty
        double difference = fabs(section_melting_temp - target_temp);

        // Apply scoring similar to anneal function
        if (difference <= 4.0) {
            total_score += difference;
        } else {
            total_score += pow(difference, 2);
        }
    }

    return total_score;
}

/*!
 * \brief Accessibility score.
 * Used to calculate the accessibility of the cutsite in the RNA sequence.
 * ViennaRNA library used to fold the RNA sequence w/o constraints.
 * Score is now calculated using section-based analysis: identifies contiguous
 * accessible regions within each binding arm and scores them individually,
 * providing more nuanced scoring than the previous binary approach.
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
 * \param target_temp Target temperature for binding
 * \param score Out variable for accessibility score
 * \return Status Code
 */
DLL_PUBLIC R_STATUS accessibility(const char* substrate_sequence, const char* substrate_structure, const char* folded_structure, const float na_concentration, const float probe_concentration, const float target_temp, /*out*/ float& score)
{
    R_STATUS status;

    // validate input sequence
    status = validate_sequence(substrate_sequence);
    if (status != R_SUCCESS::R_STATUS_OK) {
        return status;
    }

    std::string local_sequence = substrate_sequence;
    std::string local_structure = substrate_structure;
    std::string local_folded = folded_structure;

    if (local_sequence.length() != local_structure.length() ||
        local_structure.length() != local_folded.length()) {
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
    double total_accessibility_score = 0.0;
    bool has_any_accessible_sections = false;
    bool all_arms_fully_accessible = true;

    // Process each binding arm individually
    for (std::sregex_iterator i = std::sregex_iterator(local_structure.begin(), local_structure.end(), base_regex);
        i != std::sregex_iterator();
        ++i)
    {
        std::smatch match = *i;

        // Extract arm sequence and corresponding folded structure
        std::string arm_sequence = local_sequence.substr(match.position(), match.length());
        std::string arm_folded_structure = local_folded.substr(match.position(), match.length());

        // Skip single nucleotide arms to avoid melting library issues
        if (arm_sequence.length() <= 1) {
            continue;
        }

        // Check if entire arm is accessible (all positions are '.')
        bool arm_fully_accessible = true;
        for (char c : arm_folded_structure) {
            if (c != '.') {
                arm_fully_accessible = false;
                all_arms_fully_accessible = false;
                break;
            }
        }

        if (arm_fully_accessible) {
            // Entire arm is accessible - calculate ideal annealing score
            std::lock_guard<std::mutex> lock(melting_mutex);
            double arm_melting_temp = melting(arm_sequence.c_str(), na_concentration, probe_concentration);
            double difference = fabs(arm_melting_temp - target_temp);

            if (difference <= 4.0) {
                total_accessibility_score += difference;
            } else {
                total_accessibility_score += pow(difference, 2);
            }
            has_any_accessible_sections = true;
        } else {
            // Find accessible sections within this arm
            std::vector<AccessibleSection> accessible_sections = find_accessible_sections(arm_sequence, arm_folded_structure);

            if (!accessible_sections.empty()) {
                has_any_accessible_sections = true;

                // Calculate accessibility score for this arm's accessible sections
                double arm_score = calculate_section_accessibility_score(accessible_sections, na_concentration, probe_concentration, target_temp);
                total_accessibility_score += arm_score;
            } else {
                // No accessible sections in this arm - apply full penalty
                std::lock_guard<std::mutex> lock(melting_mutex);
                double arm_melting_temp = melting(arm_sequence.c_str(), na_concentration, probe_concentration);
                double difference = fabs(arm_melting_temp - target_temp);

                if (difference <= 4.0) {
                    total_accessibility_score += difference;
                } else {
                    total_accessibility_score += pow(difference, 2);
                }
            }
        }
    }

    // If all arms are fully accessible, return perfect score
    if (all_arms_fully_accessible) {
        score = 0.0f;
        return R_SUCCESS::R_STATUS_OK;
    }

    // If no accessible sections found in any arm, apply additional penalty
    if (!has_any_accessible_sections) {
        total_accessibility_score *= 1.5;
    }

    score = static_cast<float>(total_accessibility_score);
    return R_SUCCESS::R_STATUS_OK;
}

}