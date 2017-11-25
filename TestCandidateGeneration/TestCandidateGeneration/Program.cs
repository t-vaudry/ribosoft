using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestCandidateGeneration
{
    class Program
    {
        public static List<Tuple<int, int>> gNeighboursIndices = new List<Tuple<int, int>>();

        public static Ribozyme gRibozyme;

        public static List<Sequence> gSequences = new List<Sequence>();
        public static List<Sequence> gSubstrateSequences = new List<Sequence>();

        //Holds the index equivalencies of bonding ribozyme/substrate pairs
        public static List<Tuple<int, int>> gRibozymeSubstrateIndexPairs = new List<Tuple<int, int>>();

        public static List<Sequence> gSequencesToSend = new List<Sequence>();

        public static String gInputRNASequence;

        public static List<List<Node>> gNodesAtDepthSequence = new List<List<Node>>();
        public static List<List<Node>> gNodesAtDepthCutSite = new List<List<Node>>();

        public static Stack<uint> gOpenBondIndices = new Stack<uint>();
        public static Stack<uint> gOpenPseudoKnotIndices = new Stack<uint>();
        static void Main(string[] args)
        {
            //*********************
            //
            //A- Get all permutations of ribozyme sequence:
            //  a) Generate structure
            //  b) Do regular traversal BUT if current idx is in list of RNA link indices, just add X and continue
            //B- Generate cut site tree
            //C- Traverse cut site tree to generate a list of all possible cut sites
            //D- Foreach cut site, if not found in RNA input, delete.
            //E- Foreach remaining cutsite:
            //  a) Find complement
            //      i) foreach ribozyme sequence (A-), copy, and replace Xi with list of RNA link indices[i] (ignoring any -). Add to list to send to algo
            //F- Send list generated in E- to algo
            //
            //*********************


            //*********************
            //1- Get user input
            //*********************

            GetUserInput();

            //*********************
            //2- Generate the tree structures
            //*********************

            GenerateStructure(gNodesAtDepthSequence, gRibozyme.mSequence, gRibozyme.mStructure);

            //This is making the assumption that the substrate has no bonds/pseudoknots, only empty or targets
            GenerateStructure(gNodesAtDepthCutSite, gRibozyme.mSubstrateSequence);

            //TODO: Parallelize 3&4

            //*********************
            //3- Traverse sequence tree
            //*********************

            foreach (Node rootNode in gNodesAtDepthSequence[0])
                TraverseSequence(new Sequence(gRibozyme.mSequence.Length), rootNode);

            //*********************
            //4- Traverse cut site tree
            //*********************

            foreach (Node rootNode in gNodesAtDepthCutSite[0])
                TraverseNoStructure(new Sequence(gRibozyme.mSubstrateSequence.Length), rootNode);

            //*********************
            //5- Eliminate potential cut sites that are not found on input RNA
            //*********************
            EliminateCutSites();

            //*********************
            //6- Finish generating sequences based on cut sites and create list of all permutations of sequences + accepted cut sites
            //*********************

            CompleteSequencesWithCutSiteInfo();


            Console.WriteLine("Amount of sequences (no cut site): {0}", gSequences.Count);
            Console.WriteLine("Amount of sequences to send: {0}", gSequencesToSend.Count);

            Console.WriteLine("\nAccepted cut sites: ");
            foreach (Sequence cutsite in gSubstrateSequences)
                Console.WriteLine(cutsite.GetString());

            //Console.WriteLine("Sending sequences: ");
            //foreach (Sequence seq in gSequencesToSend)
            //    Console.WriteLine(seq.GetString());

            Console.ReadLine();
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
                    char structure = inputStructure[i];

                    //If nucleotide is NOT a target
                    if (!IsTarget(structure))
                    {
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
                                gNeighboursIndices.Add(Tuple.Create(i, (int)neighbourIndex));
                                break;
                            case '[': //Start a pseudoknot
                                gOpenPseudoKnotIndices.Push((uint)i);
                                break;
                            case ']': //Close a pseudoknot
                                neighbourIndex = gOpenPseudoKnotIndices.Pop();
                                gNeighboursIndices.Add(Tuple.Create(i, (int)neighbourIndex));
                                break;
                            default: //Should not happen
                                Console.WriteLine("Unrecognized structure symbol encountered.");
                                break;
                        }
                    }
                }

                //Make a node for each possible nucleotide at the current depth (if input is not a base) and set its parents to be all nodes at the previous depth
                foreach (char baseChar in nucleotide.mBases)
                {
                    Node currentNode = new Node(new Nucleotide(baseChar), i, neighbourIndex);

                    if (i > 0)
                        currentNode.SetParents(nodesAtDepth[i - 1]);

                    //Add this node to the nodes at the current depth
                    depth_i.Add(currentNode);
                }

                //Go to the previous depth and set all nodes' children to nodes of current depth
                if (i > 0)
                {
                    foreach (Node node in nodesAtDepth[i - 1])
                    {
                        node.SetChildren(depth_i);
                    }
                }

                //If we have found the second element of a neighbour pair, set the neighbour index of both nodes in pair
                if (neighbourIndex != uint.MaxValue)
                {
                    foreach (Node firstNeighbour in nodesAtDepth[(int)neighbourIndex])
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

        static public void TraverseNoStructure(Sequence currentSequence, Node currentNode)
        {
            currentSequence.mNucleotides.Add(currentNode.mNucleotide);

            if (currentNode.mChildren.Count == 0) //Leaf
            {
                gSubstrateSequences.Add(currentSequence);
            }
            else
            {
                foreach (Node child in currentNode.mChildren)
                    TraverseNoStructure(new Sequence(currentSequence), child);
            }
        }

        static public void TraverseSequence(Sequence currentSequence, Node currentNode)
        {
            currentSequence.mNucleotides.Add(currentNode.mNucleotide);

            //If leaf, add sequence to list
            if (currentNode.mChildren.Count == 0) //Leaf
            {
                if (gSequences.Contains(currentSequence))
                {
                    Console.WriteLine("Inserting duplicate!");
                }
                gSequences.Add(currentSequence);
            }
            else
            {
                //If child is going to be linked to RNA, just continue
                if (IsTarget(gRibozyme.mStructure[currentNode.mDepth + 1]))
                {
                    Node child = new Node(currentNode.mChildren[0]);
                    child.mNucleotide.SetSymbol('-');
                    TraverseSequence(new Sequence(currentSequence), child);
                }
                //Else if the children have a neighbour (will all be the same, so just check 0) and that neighbour has already been set, we must choose 
                else if (currentNode.mChildren[0].mNeighbourIndex < currentNode.mDepth + 1)
                {
                    //Get the complement of the base at the specified neighbour index in the input string
                    char requiredBase = GetComplement(currentSequence.GetCharAt((int)currentNode.mChildren[0].mNeighbourIndex));
                    bool found = false;
                    foreach (Node child in currentNode.mChildren)
                    {
                        if (child.mNucleotide.mSymbol == requiredBase)
                        {
                            TraverseSequence(new Sequence(currentSequence), child);
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
                    foreach (Node child in currentNode.mChildren)
                        TraverseSequence(new Sequence(currentSequence), child);
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

        static public String GetComplement(String s)
        {
            StringBuilder complement = new System.Text.StringBuilder();

            foreach (char c in s)
                complement.Append(GetComplement(c));

            return complement.ToString();
        }

        static public void EliminateCutSites()
        {
            //Eliminate the cut sites that are not found in the input RNA sequence
            for (int i = gSubstrateSequences.Count - 1; i > -1; i--)
            {
                if (gInputRNASequence.IndexOf(gSubstrateSequences[i].GetString()) == -1)
                {
                    //Console.WriteLine("Eliminating cut site: not found in RNA sequence.");
                    gSubstrateSequences.RemoveAt(i);
                }
            }
        }

        static public void GetUserInput()
        {
            FileStream filestream = new System.IO.FileStream("C:\\Users\\anita\\Desktop\\pistol_model.txt",
                                          System.IO.FileMode.Open,
                                          System.IO.FileAccess.Read,
                                          System.IO.FileShare.ReadWrite);
            var file = new System.IO.StreamReader(filestream, System.Text.Encoding.UTF8, true, 128);

            String inputRibozymeSequence = file.ReadLine();
            String inputRibozymeStructure = file.ReadLine();
            String inputSubstrateSequence = file.ReadLine();
            String inputSubstrateStructure = file.ReadLine();

            Debug.Assert(inputRibozymeSequence.Length == inputRibozymeStructure.Length);
            Debug.Assert(inputSubstrateSequence.Length == inputSubstrateStructure.Length);

            //Create ribozyme
            gRibozyme = new Ribozyme(inputRibozymeSequence, inputRibozymeStructure, inputSubstrateSequence, inputSubstrateStructure);

            while (!file.EndOfStream)
                gInputRNASequence += file.ReadLine();
        }

        static public void CompleteSequencesWithCutSiteInfo()
        {
            //First, build the mapping between substrate and ribozyme
            for (int i = 0; i < gRibozyme.mSubstrateStructure.Length; i++)
            {
                //If the substrate is not part of the target, continue
                if (gRibozyme.mSubstrateStructure[i] == '.')
                    continue;

                //If it is not a '.', we are expecting it to be part of the target
                Debug.Assert(IsTarget(gRibozyme.mSubstrateStructure[i]));

                //Now find the base in the input sequence that binds to this (aka has the same struc value)
                bool foundMatch = false;
                for (int j = 0; j < gRibozyme.mStructure.Length; j++)
                {
                    if (gRibozyme.mStructure[j] == gRibozyme.mSubstrateStructure[i])
                    {
                        foundMatch = true;
                        gRibozymeSubstrateIndexPairs.Add(Tuple.Create(j, i));
                    }
                }
                Debug.Assert(foundMatch);
            }

            //Complete each sequence with the complement of the substrate at the target positions
            foreach (Sequence substrate in gSubstrateSequences)
            {
                String substrateComplement = GetComplement(substrate.GetString());

                foreach (Sequence ribozymeSequence in gSequences)
                {
                    Sequence newSequence = new Sequence(ribozymeSequence);
                    bool success = true;

                    //For each element in the ribozyme sequence that is part of the traget area, check if it is possible to bond with the substrate
                    foreach (Tuple<int, int> indexPair in gRibozymeSubstrateIndexPairs)
                    {
                        int riboIdx = indexPair.Item1;
                        int substrateIdx = indexPair.Item2;

                        Nucleotide ribozymeNucleotide = new Nucleotide(gRibozyme.mSequence[riboIdx]);
                        if (ribozymeNucleotide.mBases.Contains(substrateComplement[substrateIdx]))
                        {
                            newSequence.mNucleotides[riboIdx] = new Nucleotide(substrateComplement[substrateIdx]);
                        }
                        else
                        {
                            success = false;
                            break;
                        }
                    }

                    //If all target elements can successfully bond, add this sequence to the list
                    if (success)
                        gSequencesToSend.Add(newSequence);
                }
            }
        }

        static public bool IsTarget(char b)
        {
            return ((b >= 'a' && b <= 'z') ||
                        (b >= 'A' && b <= 'Z') ||
                        (b >= '0' && b <= '9'));
        }
    }
}
