using System;
using System.Collections.Generic;
using System.Text;

namespace TestCandidateGeneration
{
    class Node
    {
        public Nucleotide mNucleotide;
        public List<Node> mParents;
        public List<Node> mChildren;
        public uint mNeighbourIndex;
        public int mDepth;
        public Node()
        {
            mDepth = int.MaxValue;
            mParents = new List<Node>();
            mChildren = new List<Node>();
        }

        public Node(Nucleotide nucleotide, int depth, uint neighbourIdx = uint.MaxValue)
        {
            mNucleotide = nucleotide;
            mParents = new List<Node>();
            mChildren = new List<Node>();
            mDepth = depth;
            mNeighbourIndex = neighbourIdx;
        }

        public Node(Node otherNode)
        {
            mNucleotide = otherNode.mNucleotide;
            mParents = otherNode.mParents;
            mChildren = otherNode.mChildren;
            mDepth = otherNode.mDepth;
            mNeighbourIndex = otherNode.mNeighbourIndex;
        }

        public void SetChildren(List<Node> children)
        {
            mChildren = children;
        }
        public void SetParents(List<Node> parents)
        {
            mParents = parents;
        }

        public void SetNeighbourIndex(uint idx)
        {
            mNeighbourIndex = idx;
        }
    }
}
