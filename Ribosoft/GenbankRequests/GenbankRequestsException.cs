using System;
using System.Runtime.Serialization;

namespace Ribosoft.GenbankRequests
{
    [Serializable]
    public class GenbankRequestsException : Exception
    {
        public GenbankRequestsException()
        {

        }

        public GenbankRequestsException(string message)
            : base(message)
        {

        }

        public GenbankRequestsException(string message, Exception inner)
            : base(message, inner)
        {

        }

        protected GenbankRequestsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
