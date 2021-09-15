#include "dll.h"

//! \namespace ribosoft
namespace ribosoft {

/*! \typedef R_STATUS
 *  \brief Status Code
 */
typedef signed int R_STATUS;

/*! \enum R_SUCCESS
 * \brief Success Codes
 * SUCCESS : Range (0)-(1000)
 */
enum R_SUCCESS : R_STATUS {
    R_STATUS_OK                    =     0, //!< OK
    R_SUCCESS_LAST                 =  1000, //!< NON-ASSOCIATED CODE
};

/*! \enum R_APPLICATION_ERROR
 *\brief Application Error Codes
 * APPLICATION ERROR : Range (-1)-(-999)
 */
enum R_APPLICATION_ERROR : R_STATUS {
    R_INVALID_PARAMETER            =    -1, //!< General invalid parameter
    R_INVALID_NUCLEOTIDE           =    -2, //!< Invalid nucleotide found in sequence
    R_INVALID_STRUCT_ELEMENT       =    -3, //!< Invalid element found in structure
    R_EMPTY_PARAMETER              =    -4, //!< Parameter sent is empty!
    R_BAD_PAIR_MATCH               =    -5, //!< Pair matching is incorrect
    R_FITNESS_VALUE_LENGTHS_DIFFER =    -6, //!< Lengths of FitnessValues differ
    R_EMPTY_CANDIDATE_LIST         =    -7, //!< Candidates list is empty
    R_STRUCT_LENGTH_DIFFER         =    -8, //!< Length of structures differ
    R_OUT_OF_RANGE                 =    -9, //!< Value is out of range
    R_INVALID_TEMPLATE_LENGTH      =    -10, //!< Template length is invalid
    R_INVALID_CONCENTRATION        =    -11, //!< Concentration is out of range
    R_INVALID_ARM_LENGTH           =    -12, //!< Arm length is 1
    R_APPLICATION_ERROR_LAST       =    -999, //!< NON-ASSOCIATED CODE
};

/*! \enum R_USER_ERROR
 * \brief User Error Codes
 * USER ERROR : Range (-1000)-(-1999)
 */
enum R_USER_ERROR : R_STATUS {
    R_USER_ERROR_FIRST             = -1000, //!< NON-ASSOCIATED CODE
    R_USER_ERROR_LAST              = -1999, //!< NON-ASSOCIATED CODE
};

/*! \enum R_SYSTEM_ERROR
 * \brief System Error Codes
 * SYSTEM ERROR : Range (-2000)-(-2999)
 */
enum R_SYSTEM_ERROR : R_STATUS {
    R_SYSTEM_ERROR_FIRST           = -2000, //!< NON-ASSOCIATED CODE
    R_VIENNA_RNA_ERROR             = -2001, //!< Error by ViennaRNA, contact us with details.
    R_SYSTEM_ERROR_LAST            = -2999, //!< NON-ASSOCIATED CODE
};

}