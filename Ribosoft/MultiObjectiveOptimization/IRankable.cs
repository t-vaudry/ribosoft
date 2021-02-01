using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.MultiObjectiveOptimization
{
	/*! \interface IRankable
     * \brief Interface for ranking two objects against one another
     */
    public interface IRankable<T>
    {
    	/*! \property Rank
         * \brief Rank of current element
         */
        int Rank { get; set; }

        /*! \property Comparables
         * \brief List of comparables
         */
        IEnumerable<T> Comparables { get; }
    }
}
