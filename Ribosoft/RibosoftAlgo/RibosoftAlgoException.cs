using System;

namespace Ribosoft
{
    /*! \class RibosoftAlgoException
     * \brief Exception class used to provide details on the failures from the RibosoftAlgo nuget package
     */
    public class RibosoftAlgoException : Exception
    {
        /*! \property Code
         * \brief Status code of the exception
         */
        public R_STATUS Code { get; set; }

        /*!
         * \brief Default constructor
         */
        public RibosoftAlgoException()
            : this(R_STATUS.R_APPLICATION_ERROR_LAST)
        {
        }

        /*!
         * \brief Exception constructor, sets code
         * \param code Status code
         */
        public RibosoftAlgoException(R_STATUS code)
        {
            this.Code = code;
        }
    }
}
