using System;

namespace Ribosoft.RibosoftAlgo
{
    public class RibosoftAlgoException : Exception
    {
        public R_STATUS Code { get; set; }

        public RibosoftAlgoException(R_STATUS code)
        {
            this.Code = code;
        }

        public RibosoftAlgoException()
            : this(R_STATUS.R_APPLICATION_ERROR_LAST)
        {
        }
    }
}
