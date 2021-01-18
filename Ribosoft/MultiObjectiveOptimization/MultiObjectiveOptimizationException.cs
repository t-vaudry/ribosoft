using System;
using System.Runtime.Serialization;

namespace Ribosoft.MultiObjectiveOptimization
{
    [Serializable]
    public class MultiObjectiveOptimizationException : Exception
    {
        public R_STATUS Code { get; set; }

        public MultiObjectiveOptimizationException()
        {
            Code = R_STATUS.R_APPLICATION_ERROR_LAST;
        }

        public MultiObjectiveOptimizationException(R_STATUS code, string message)
            : base(message)
        {
            Code = code;
        }

        public MultiObjectiveOptimizationException(R_STATUS code, string message, Exception inner)
            : base(message, inner)
        {
            Code = code;
        }

        protected MultiObjectiveOptimizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}