using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.MultiObjectiveOptimization
{
    /*! \enum OptimizeType
     * \brief Optimization types
     *  MAX: Optimize for the max value
     *  MIN: Optimize for the min value
     */
    public enum OptimizeType
    {
        MAX,
        MIN
    }

    /*! \class OptimizeItem
     * \brief Object class for an optimization item
     */
    public class OptimizeItem<T> where T : IComparable<T>
    {
        /*! \property Value
         * \brief Value of the item
         */
        public T Value { get; set; }

        /*! \property Type
         * \brief Optimize for the max or min value
         */
        public OptimizeType Type { get; set; }

        /*! \property Tolerance
         * \brief Allowance of tolerance when comparing two items
         */
        public float Tolerance { get; set; }

        /*! \fn OptimizeItem
         * \brief Default constructor
         * \param value Value of item
         * \param type Type of optimization
         * \param tolerance Tolerance allowance
         */
        public OptimizeItem(T value, OptimizeType type, float tolerance)
        {
            Value = value;
            Type = type;
            Tolerance = tolerance;
        }
    }
}
