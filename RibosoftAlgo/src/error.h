#include "dll.h"

namespace ribosoft {

    typedef signed int R_STATUS;

    /* SUCCESS : Range (0)-(1000) */
    enum R_SUCCESS : R_STATUS {
        R_OK                        =     0,
        R_SUCCESS_LAST              =  1000,
    };

    /* APPLICATION ERROR : Range (-1)-(-999) */
    enum R_APPLICATION_ERROR : R_STATUS {
        R_INVALID_PARAMETER         =    -1,
        R_INVALID_NUCLEOTIDE        =    -2,
        R_INVALID_STRUCT_ELEMENT    =    -3,
        R_EMPTY_PARAMETER           =    -4,
        R_BAD_PAIR_MATCH            =    -5,
        R_APPLICATION_ERROR_LAST    =  -999,
    };

    /* USER ERROR : Range (-1000)-(-1999) */
    enum R_USER_ERROR : R_STATUS {
        R_USER_ERROR_FIRST          = -1000,
        R_USER_ERROR_LAST           = -1999,
    };

    /* SYSTEM ERROR : Range (-2000)-(-2999) */
    enum R_SYSTEM_ERROR : R_STATUS {
        R_SYSTEM_ERROR_FIRST        = -2000,
        R_SYSTEM_ERROR_LAST         = -2999,
    };

}