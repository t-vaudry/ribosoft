using System;
using System.Collections.Generic;
using System.Text;

using Ribosoft.Biology;

namespace Ribosoft.CandidateGeneration
{
    /*! \class Node
     * \brief Object class for a node within the candidate generation tree
     */
    public class Node
    {
        /*! \property Nucleotide
         * \brief Nucleotide base of the current node
         */
        public Nucleotide Nucleotide { get; set; }

        /*! \property Parents
         * \brief List of parent nodes attached to this node
         */
        public List<Node> Parents { get; set; }

        /*! \property Children
         * \brief List of child nodes attached to this node
         */
        public List<Node> Children { get; set; }

        /*! \property NeighbourIndex
         * \brief Index of the neighbor node
         */
        public int? NeighbourIndex { get; set; }

        /*! \property Depth
         * \brief Depth in the node tree
         */
        public int Depth { get; private set; }
        
        /*!
         * \brief Default constructor
         */
        public Node()
        {
            Depth = -1;
            Parents = new List<Node>();
            Children = new List<Node>();
        }

        /*!
         * \brief Constructor
         * \param nucleotide Nucleotide base for this node
         * \param depth Current node depth
         * \param neighbourIdx Neighbour index
         */
        public Node(Nucleotide nucleotide, int depth, int? neighbourIdx = null)
        {
            Nucleotide = nucleotide;
            Parents = new List<Node>();
            Children = new List<Node>();
            Depth = depth;
            NeighbourIndex = neighbourIdx;
        }

        /*!
         * \brief Copy constructor
         * \param otherNode Node to be copied
         */
        public Node(Node otherNode)
        {
            Nucleotide = otherNode.Nucleotide;
            Parents = otherNode.Parents;
            Children = otherNode.Children;
            Depth = otherNode.Depth;
            NeighbourIndex = otherNode.NeighbourIndex;
        }
    }
}
