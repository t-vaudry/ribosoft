using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft
{
    /* SUCCESS            : Range (0)-(1000)      */
    /* APPLICATION ERROR  : Range (-1)-(-999)     */
    /* USER ERROR         : Range (-1000)-(-1999) */
    /* SYSTEM ERROR       : Range (-2000)-(-2999) */
    public enum R_ERROR : int
    {
        R_OK                        =  0,
        R_INVALID_PARAMETER         = -1,
        R_INVALID_NUCLEOTIDE        = -2,
        R_INVALID_STRUCT_ELEMENT    = -3,
        R_EMPTY_PARAMETER           = -4,
        R_BAD_PAIR_MATCH            = -5,
    };
}
