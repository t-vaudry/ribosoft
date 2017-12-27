using System;
using System.Runtime.Serialization;

namespace Ribosoft.MultiObjectiveOptimization
{
    [Serializable]
    public class MultiObjectiveOptimizationException : Exception
    {
        public MultiObjectiveOptimizationException()
        {

        }

        public MultiObjectiveOptimizationException(string message)
            : base(message)
        {

        }

        public MultiObjectiveOptimizationException(string message, Exception inner)
            : base(message, inner)
        {

        }

        protected MultiObjectiveOptimizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}