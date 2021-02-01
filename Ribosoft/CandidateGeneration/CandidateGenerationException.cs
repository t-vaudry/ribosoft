using System;
using System.Runtime.Serialization;

namespace Ribosoft.CandidateGeneration
{
    /*! \class CandidateGenerationException
     * \brief Exception class for candidate generation
     */
    [Serializable]
    public class CandidateGenerationException : Exception
    {
        /*!
         * \brief Default constructor
         */
        public CandidateGenerationException()
        {
        }

        /*!
         * \brief Constructor
         * \param message Exception message
         */
        public CandidateGenerationException(string message)
            : base(message)
        {
        }

        /*!
         * \brief Constructor
         * \param message Exception message
         * \param inner Exception to copy
         */
        public CandidateGenerationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /*!
         * \brief Constructor
         * \param info Serialization information
         * \param context Streaming context
         */
        protected CandidateGenerationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}