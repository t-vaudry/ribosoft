using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TestCandidateGeneration
{
    class Program
    {
        public enum SequenceType
        {
            Sequence,
            CutSite
        };

        public static Ribozyme gRibozyme;

        public static List<Sequence> gSequences = new List<Sequence>();
        public static List<Sequence> gCutSiteSequences = new List<Sequence>();

        public static String gInputRibozymeSequence;
        public static String gInputRibozymeStructure;
        public static String gInputRibozymeCutSite;

        public static String gInputRNASequence;

        public static List<List<Node>> gNodesAtDepthSequence = new List<List<Node>>();
        public static List<List<Node>> gNodesAtDepthCutSite = new List<List<Node>>();

        public static Stack<uint> gOpenBondIndices = new Stack<uint>();
        public static Stack<uint> gOpenPseudoKnotIndices = new Stack<uint>();
        static void Main(string[] args)
        {
            //*********************
            //
            //1- Get all permutations of ribozyme sequence:
            //  a) Generate structure
            //  b) Do regular traversal BUT if current idx is in list of RNA link indices, just add X and continue
            //2- Generate cut site tree
            //3- Traverse cut site tree to generate a list of all possible cut sites
            //4- Foreach cut site, if not found in RNA input, delete
            //5- Foreach remaining cutsite:
            //  a) Find complement
            //      i) foreach ribozyme sequence (1-), copy, and replace Xi with list of RNA link indices[i] (ignoring any -). Add to list to send to algo
            //6- Send list generated in 5- to algo
            //
            //*********************


            //*********************
            //1- Get user input
            //*********************

            Console.WriteLine("Enter ribozyme sequence: \n");
            gInputRibozymeSequence = Console.ReadLine();

            Console.WriteLine("Enter ribozyme secondary structure: \n");
            gInputRibozymeStructure = Console.ReadLine();

            Console.WriteLine("Enter cut site sequence: \n");
            gInputRibozymeCutSite = Console.ReadLine();

            Console.WriteLine("Enter RNA sequence: \n");
            gInputRNASequence = Console.ReadLine();

            Debug.Assert(gInputRibozymeStructure.Length == gInputRibozymeSequence.Length);

            gRibozyme = new Ribozyme(gInputRibozymeSequence, gInputRibozymeStructure, gInputRibozymeCutSite);

            //*********************
            //2- Generate the tree structures
            //*********************

            Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            GenerateStructure(gNodesAtDepthSequence, gInputRibozymeSequence, gInputRibozymeStructure);
            GenerateStructure(gNodesAtDepthCutSite, gInputRibozymeCutSite);
            watch.Stop();
            Console.WriteLine(String.Format("Generation time: {0}{1}", watch.ElapsedMilliseconds, "ms"));

            //*********************
            //
            //3- Traverse the tree:
            //Note: a) and b) can be done in parallel
            //  a) Generate all permutations of cutsite
            //      i) foreach permuation, search for exact match in RNA sequence
            //          -if not found, reject permutation
            //  b) Generate all permuations of rybozyme sequence (no cutsite)
            //  c) Generate all permutations of sequence + found cut site  and send to Algorithms
            //      -foreach approved cutsite, geenrate all matching sequence candidates
            //
            //*********************

            //Generate all permutations of rybozyme sequence
            foreach (Node rootNode in gNodesAtDepthSequence[0])
                Traverse(new Sequence(gInputRibozymeSequence.Length), rootNode, SequenceType.Sequence);

            //Generate all permutations for cutsite
            watch.Start();
            foreach (Node rootNode in gNodesAtDepthCutSite[0])
                Traverse(new Sequence(gInputRibozymeCutSite.Length), rootNode, SequenceType.CutSite);
            watch.Stop();
            Console.WriteLine(String.Format("Traversal time: {0}{1}", watch.ElapsedMilliseconds, "ms"));

            Console.WriteLine("Sequences: \n");
            foreach (Sequence sequence in gSequences)
            {
                //Debug.Assert(sequence.mNucleotides.Count == gInputSequence.Length);
                Console.WriteLine(sequence.GetString());
            }
            Console.WriteLine(String.Format("Amount Generated: {0}", gSequences.Count));

            Console.ReadLine();
        }

        static public void GetPermutations(Sequence currentSequence, int currentIndex, Nucleotide currentNucleotide)
        {
            for (int i = currentIndex; i < gInputRibozymeSequence.Length; i++)
            {
                Debug.Assert(currentNucleotide.mSymbol != 'X');
                if (!currentNucleotide.mIsBase)
                {
                    foreach (char potentialBase in currentNucleotide.mBases)
                    {
                        GetPermutations(new Sequence(currentSequence), currentIndex, new Nucleotide(potentialBase));
                    }
                    return;
                }
                else
                {
                    currentSequence.mNucleotides.Add(currentNucleotide);
                    currentIndex++;
                    if (currentIndex < gInputRibozymeSequence.Length)
                        currentNucleotide = new Nucleotide(gInputRibozymeSequence[currentIndex]);
                }
            }
            gSequences.Add(currentSequence);
        }

        static public void GenerateStructure(List<List<Node>> nodesAtDepth, String inputSequence, String inputStructure = null)
        {
            int depth = inputSequence.Length;

            for (int i = 0; i < depth; i++)
            {
                //List for nodes at current depth
                List<Node> depth_i = new List<Node>();

                //Get input at index (nucleotide and secondary structure)
                Nucleotide nucleotide = new Nucleotide(inputSequence[i]);

                //The neighbour (of a bond or pseudoknot)
                uint neighbourIndex = uint.MaxValue;
                if (inputStructure != null)
                {
                    char structure = gInputRibozymeStructure[i];

                    //Determine if the nucleotide has a neighbour (bond or pseudoknot)
                    switch (structure)
                    {
                        case '.': //Nothing to do
                            break;
                        case '(': //Start an open bond
                            gOpenBondIndices.Push((uint)i);
                            break;
                        case ')': //Close an open bond
                            neighbourIndex = gOpenBondIndices.Pop();
                            break;
                        case '{': //Start a pseudoknot
                            gOpenPseudoKnotIndices.Push((uint)i);
                            break;
                        case '}': //Close a pseudoknot
                            neighbourIndex = gOpenPseudoKnotIndices.Pop();
                            break;
                        default: //Should not happen
                            Console.WriteLine("Unrecognized structrue symbol encountered.");
                            break;
                    }
                }

                //Make a node for each possible nucleotide at the current depth (if input is not a base) and set its parents to be all nodes at the previous depth
                foreach (char baseChar in nucleotide.mBases)
                {
                    Node currentNode = new Node(new Nucleotide(baseChar), i, neighbourIndex);

                    if (i>0)
                        currentNode.SetParents(nodesAtDepth[i - 1]);

                    //Add this node to the nodes at the current depth
                    depth_i.Add(currentNode);
                }

                //Go to the previous depth and set all nodes' children to nodes of current depth
                if (i>0)
                {
                    foreach (Node node in nodesAtDepth[i - 1])
                    {
                        node.SetChildren(depth_i);
                    }
                }

                //If we have found the second element of a neighbour pair, set the neighbour index of both nodes in pair
                if (neighbourIndex != uint.MaxValue)
                {
                    foreach(Node firstNeighbour in nodesAtDepth[(int)neighbourIndex])
                    {
                        firstNeighbour.SetNeighbourIndex((uint)i);
                    }
                }

                //Add all nodes created at this depth to the structue
                nodesAtDepth.Add(depth_i);
            }

            //Validate that all open parentheses have been closed
            if (gOpenBondIndices.Count != 0)
            {
                Console.WriteLine("Unclosed bond found '('. Input may be faulty.");
            }
            if (gOpenPseudoKnotIndices.Count != 0)
            {
                Console.WriteLine("Unclosed pseudoknot found '{'. Input may be faulty.");
            }
        }

        static public void Traverse(Sequence currentSequence, Node currentNode, SequenceType type)
        {
            currentSequence.mNucleotides.Add(currentNode.mNucleotide);

            if (currentNode.mChildren.Count == 0) //Leaf
            {
                if (type == SequenceType.Sequence)
                    gSequences.Add(currentSequence);
                else if (type == SequenceType.CutSite)
                    gCutSiteSequences.Add(currentSequence);
            }
            else
            {
                //If the children have a neighbour (will all be the same, so just check 0) and that neighbour has already been set, we must choose 
                if (currentNode.mChildren[0].mNeighbourIndex < currentNode.mDepth + 1)
                {
                    //Get the complement of the base at the specified neighbour index in the input string
                    char requiredBase = GetComplement(currentSequence.GetCharAt((int)currentNode.mChildren[0].mNeighbourIndex));
                    bool found = false;
                    foreach (Node child in currentNode.mChildren)
                    {
                        if (child.mNucleotide.mSymbol == requiredBase)
                        {
                            Traverse(new Sequence(currentSequence), child, type);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Debug.Assert(false, "Neighbours don't match!");
                    }
                }
                else
                {
                    //TODO: Foreach VALID child (check against cut site)
                    foreach (Node child in currentNode.mChildren)
                        Traverse(new Sequence(currentSequence), child, type);
                }
            }
        }

        static public char GetComplement(char b)
        {
            switch (b)
            {
                case 'A':
                    return 'U';
                case 'U':
                    return 'A';
                case 'G':
                    return 'C';
                case 'C':
                    return 'G';
                default:
                    Console.WriteLine("Cannot get complement of invalid base.");
                    return 'X';
            }
        }
    }
}
