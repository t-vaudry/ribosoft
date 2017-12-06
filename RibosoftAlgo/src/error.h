#include <dll.h>

typedef signed int R_ERROR;

/* SUCCESS : Range (0)-(1000) */
enum R_SUCCESS : R_ERROR {
    RIBOSOFT_OK                 =  0,
};

/* APPLICATION ERROR : Range (-1)-(-999) */
enum R_APPLICATION_ERROR : R_ERROR {
    R_INVALID_PARAMETER         = -1,
    R_INVALID_NUCLEOTIDE        = -2,
    R_INVALID_STRUCT_ELEMENT    = -3,
    R_EMPTY_PARAMETER           = -4,
    R_BAD_PAIR_MATCH            = -5,
};

/* USER ERROR : Range (-1000)-(-1999) */

/* SYSTEM ERROR : Range (-2000)-(-2999) */