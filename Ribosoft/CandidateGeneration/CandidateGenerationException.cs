using System;
using System.Runtime.Serialization;

namespace Ribosoft.CandidateGeneration
{
    [Serializable]
    public class CandidateGenerationException : Exception
    {
        public CandidateGenerationException()
        {

        }

        public CandidateGenerationException(string message)
            : base(message)
        {

        }

        public CandidateGenerationException(string message, Exception inner)
            : base(message, inner)
        {

        }

        protected CandidateGenerationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
