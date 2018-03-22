using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.MultiObjectiveOptimization
{
    public interface IRankable<T>
    {
        int Rank { get; set; }

        IEnumerable<T> Comparables { get; }
    }
}
