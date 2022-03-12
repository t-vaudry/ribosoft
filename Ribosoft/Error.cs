using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft
{
    /*! \enum R_STATUS
     * \brief Status codes
     * SUCCESS            : Range (0)-(1000)
     * APPLICATION ERROR  : Range (-1)-(-999)
     * USER ERROR         : Range (-1000)-(-1999)
     * SYSTEM ERROR       : Range (-2000)-(-2999)
     */
    public enum R_STATUS : int
    {
        /* SUCCESS */
        R_STATUS_OK                    =     0,
        R_SUCCESS_LAST                 =  1000,

        /* APPLICATION ERROR */
        R_INVALID_PARAMETER            =    -1,
        R_INVALID_NUCLEOTIDE           =    -2,
        R_INVALID_STRUCT_ELEMENT       =    -3,
        R_EMPTY_PARAMETER              =    -4,
        R_BAD_PAIR_MATCH               =    -5,
        R_FITNESS_VALUE_LENGTHS_DIFFER =    -6,
        R_EMPTY_CANDIDATE_LIST         =    -7,
        R_STRUCT_LENGTH_DIFFER         =    -8,
        R_OUT_OF_RANGE                 =    -9,
        R_INVALID_TEMPLATE_LENGTH      =    -10,
        R_INVALID_CONCENTRATION        =    -11,
        R_APPLICATION_ERROR_LAST       =  -999,

        /* USER ERROR */
        R_USER_ERROR_FIRST             = -1000,
        R_USER_ERROR_LAST              = -1999,

        /* SYSTEM ERROR */
        R_SYSTEM_ERROR_FIRST           = -2000,
        R_VIENNA_RNA_ERROR             = -2001,
        R_SYSTEM_ERROR_LAST            = -2999,
    }

    /*! \class RibosoftException
     * \brief Exception class to retrieve details from the Ribosoft application
     */
    public class RibosoftException : Exception
    {
        /*! \property Code
         * \brief Status code of the exception
         */
        public R_STATUS Code { get; set; }

        /*!
         * \brief Exception constructor, sets code
         * \param code Status code
         * \param message Status message
         */
        public RibosoftException(R_STATUS code, string message)
            : base(message)
        {
            this.Code = code;
        }

        /*!
         * \brief Default constructor
         */
        public RibosoftException()
            : this(R_STATUS.R_APPLICATION_ERROR_LAST, "")
        {
        }
    }
}
