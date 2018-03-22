using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.MultiObjectiveOptimization
{
    public enum OptimizeType
    {
        MAX,
        MIN
    }

    public class OptimizeItem<T> where T : IComparable<T>
    {
        public T Value { get; set; }
        public OptimizeType Type { get; set; }

        public OptimizeItem(T value, OptimizeType type)
        {
            Value = value;
            Type = type;
        }
    }
}
