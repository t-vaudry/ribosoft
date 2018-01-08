using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ribosoft
{
    public static class Validation
    {
        public static R_STATUS ValidateSequence(string sequence, bool baseOnly)
        {
            if (sequence.Count() == 0)
            {
                return R_STATUS.R_EMPTY_PARAMETER;
            }

            string pattern = baseOnly? @"[^ACGU]+" : @"[^ACGUTWSMKRYBDHVN]+";
            Regex rgx = new Regex(pattern);
            if (rgx.IsMatch(sequence))
            {
                return R_STATUS.R_INVALID_NUCLEOTIDE;
            }

            return R_STATUS.R_STATUS_OK;
        }

        public static R_STATUS ValidateStructure(string structure, bool canHaveTarget)
        {
            if (structure.Count() == 0)
            {
                return R_STATUS.R_EMPTY_PARAMETER;
            }

            string pattern = canHaveTarget ? @"[^.()[]a-z0-9]+" : @"[^.()[]]+";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            if (rgx.IsMatch(structure))
            {
                return R_STATUS.R_INVALID_STRUCT_ELEMENT;
            }

            uint OpenDoubleBondCount = 0;
            uint OpenPseudoKnotCount = 0;

            foreach (char struc in structure)
            {
                //If nucleotide is NOT a target
                if (!canHaveTarget || !IsTarget(struc))
                {
                    //Determine if the nucleotide has a neighbour (bond or pseudoknot)
                    switch (struc)
                    {
                        case '.': //Nothing to do
                            break;
                        case '(': //Start an open bond
                            OpenDoubleBondCount++;
                            break;
                        case ')': //Close an open bond
                            if (OpenDoubleBondCount > 0)
                            {
                                OpenDoubleBondCount--;
                            }
                            else
                            {
                                return R_STATUS.R_BAD_PAIR_MATCH;
                            }
                            break;
                        case '[': //Start a pseudoknot
                            OpenPseudoKnotCount++;
                            break;
                        case ']': //Close a pseudoknot
                            if (OpenPseudoKnotCount > 0)
                            {
                                OpenPseudoKnotCount--;
                            }
                            else
                            {
                                return R_STATUS.R_BAD_PAIR_MATCH;
                            }
                            break;
                        default: //Should not happen
                            return R_STATUS.R_INVALID_STRUCT_ELEMENT;
                    }
                }
            }

            if (OpenDoubleBondCount != 0 || OpenPseudoKnotCount != 0)
            {
                return R_STATUS.R_BAD_PAIR_MATCH;
            }

            return R_STATUS.R_STATUS_OK;
        }

        public static bool IsTarget(char b)
        {
            return ((b >= 'a' && b <= 'z') ||
                        (b >= 'A' && b <= 'Z') ||
                        (b >= '0' && b <= '9'));
        }
    }
}
