using System;
using System.Runtime.Serialization;

namespace Ribosoft.MultiObjectiveOptimization
{
    /*! \class MultiObjectiveOptimizationException
     * \brief Exception class for multi-objective optimization
     */
    [Serializable]
    public class MultiObjectiveOptimizationException : Exception
    {
        /*! \property Code
         * \brief Exception status code
         */
        public R_STATUS Code { get; set; }

        /*!
         * \brief Default constructor
         */
        public MultiObjectiveOptimizationException()
        {
            Code = R_STATUS.R_APPLICATION_ERROR_LAST;
        }

        /*!
         * \brief Constructor
         * \param code Exception status code
         * \param message Exception message
         */
        public MultiObjectiveOptimizationException(R_STATUS code, string message)
            : base(message)
        {
            Code = code;
        }

        /*!
         * \brief Copy constructor
         * \param code Exception status code
         * \param message Exception message
         * \param inner Exception to copy
         */
        public MultiObjectiveOptimizationException(R_STATUS code, string message, Exception inner)
            : base(message, inner)
        {
            Code = code;
        }

        /*!
         * \brief Serialized constructor
         * \param info Serialized information
         * \param context Streaming context
         */
        protected MultiObjectiveOptimizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}