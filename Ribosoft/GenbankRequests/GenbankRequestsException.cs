using System;
using System.Runtime.Serialization;

namespace Ribosoft.GenbankRequests
{
    /*! \class GenbankRequestsException
     * \brief Exception class to retrieve errors from the GenBank requests
     */
    [Serializable]
    public class GenbankRequestsException : Exception
    {
        /*!
         * \brief Default constructor
         */
        public GenbankRequestsException()
        {
        }

        /*!
         * \brief Constructor
         * \param message Exception message
         */
        public GenbankRequestsException(string message)
            : base(message)
        {
        }

        /*!
         * \brief Copy constructor
         * \param message Exception message
         * \param inner Exception to copy
         */
        public GenbankRequestsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /*!
         * \brief Serialized constructor
         * \param info Serialized exception info
         * \param context Streaming context
         */
        protected GenbankRequestsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}