#include "dll.h"

#include <string.h>
//#include <iostream>

#include <regex>
#include <iterator>
#include <cmath>
#include <vector>
//#include <tuple>

#include "functions.h"

extern "C"

{
	DLL_PUBLIC float anneal(const char* sequence, const char* structure, float na_concentration) {

		std::string local_sequence = sequence;
		std::string local_structure = structure;

		// Constants (these will be renamed once we know where they come from)
		const float SEVENTYNINEPOINTEIGHT = 79.8;
		const float EIGHTEENPOINTFIVE = 18.5;
		const float FIFTYEIGHTPOINTFOUR = 58.4;
		const float ELEVENPOINTEIGHT = 11.8;
		const float EIGHTHUNDREDTWENTY = 820;
		const float TWO = 2.0;

		std::regex base_regex("[0-9a-zA-Z]+");
		auto substruct_begin = std::sregex_iterator(local_structure.begin(), local_structure.end(), base_regex);
		auto substruct_end = std::sregex_iterator();

		int num_substrings = distance(substruct_begin, substruct_end);

		//std::string *substrings;
		std::vector<std::string> substrings;

		int substr_index = 0;
		for (std::sregex_iterator i = substruct_begin; i != substruct_end; ++i) {
			std::smatch match = *i;
			std::string match_str = match.str();
			substrings.push_back(local_sequence.substr(match.position(), match.length()));
		}

		int* temps;
		float temp_sum = 0.0;

		for (int i = 0; i < substrings.size(); i++) {

			int a_count = 0;
			int u_count = 0;
			int c_count = 0;
			int g_count = 0;
			float wA;
			float xT;
			float zC;
			float yG;

			std::string substring = substrings.at(i);
			int length = substring.size();

			for (int j = 0; j < length; j++) {
				char base = substring[j];
				switch (base) {
				case 'a':
					a_count++;
					break;
				case 'A':
					a_count++;
					break;
				case 'u':
					u_count++;
					break;
				case 'U':
					u_count++;
					break;
				case 'c':
					c_count++;
					break;
				case 'C':
					c_count++;
					break;
				case 'g':
					g_count++;
					break;
				case 'G':
					g_count++;
					break;
				default:
					// printf("Error while counting bases: sequence contains non-base character (must be a, A, u, U, c, C, g, or G");
					return 1;
				}
			}

			wA = float(a_count);
			xT = float(u_count);
			zC = float(c_count);
			yG = float(g_count);
			float log_concentration = 0.0;
			
			try
			{
				log_concentration = log(na_concentration);
			}
			catch (const std::exception&)
			{
				// printf("Error while calculating log: concentration maybe be 0 (concentration cannot be 0)");
				return 1;
			}
			
			// Tm= 79.8 + 18.5*log10([Na+]) + (58.4 * (yG+zC)/(wA+xT+yG+zC)) + (11.8 * ((yG+zC)/(wA+xT+yG+zC))2) - (820/(wA+xT+yG+zC)) 
			float Tm = SEVENTYNINEPOINTEIGHT + EIGHTEENPOINTFIVE * log_concentration + (FIFTYEIGHTPOINTFOUR * (yG + zC) / (wA + xT + yG + zC)) + (ELEVENPOINTEIGHT * ((yG + zC) / (wA + xT + yG + zC)) * TWO) - (EIGHTHUNDREDTWENTY / (wA + xT + yG + zC));
			temp_sum += Tm;
		}
		return temp_sum;
	}
}