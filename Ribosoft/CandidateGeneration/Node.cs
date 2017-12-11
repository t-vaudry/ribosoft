using System;
using System.Collections.Generic;
using System.Text;

namespace Ribosoft.CandidateGeneration
{
    class Node
    {
        public Nucleotide Nucleotide { get; set; }
        public List<Node> Parents { get; set; }
        public List<Node> Children { get; set; }
        public int? NeighbourIndex { get; set; }
        public int Depth { get; private set; }
        public Node()
        {
            Depth = -1;
            Parents = new List<Node>();
            Children = new List<Node>();
        }

        public Node(Nucleotide nucleotide, int depth, int? neighbourIdx = null)
        {
            Nucleotide = nucleotide;
            Parents = new List<Node>();
            Children = new List<Node>();
            Depth = depth;
            NeighbourIndex = neighbourIdx;
        }

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
