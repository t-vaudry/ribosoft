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
        public static Ribozyme gRibozyme;

        public static List<Sequence> gSequences = new List<Sequence>();
        public static List<Sequence> gCutSiteSequences = new List<Sequence>();

        public static List<Sequence> gSequencesToSend = new List<Sequence>();

        public static String gInputRibozymeSequence;
        public static String gInputRibozymeStructure;
        public static String gInputRibozymeCutSite;
        public static List<int> gInputRNALinkIndices = new List<int>();

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
            //D- Foreach cut site, if not found in RNA input, delete
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

            Debug.Assert(gInputRibozymeStructure.Length == gInputRibozymeSequence.Length);

            gRibozyme = new Ribozyme(gInputRibozymeSequence, gInputRibozymeStructure, gInputRibozymeCutSite, gInputRNALinkIndices);

            //*********************
            //2- Generate the tree structures
            //*********************

            GenerateStructure(gNodesAtDepthSequence, gInputRibozymeSequence, gInputRibozymeStructure);
            GenerateStructure(gNodesAtDepthCutSite, gInputRibozymeCutSite);

            //TODO: Parallelize 3&4

            //*********************
            //3- Traverse sequence tree
            //*********************

            foreach (Node rootNode in gNodesAtDepthSequence[0])
                TraverseSequence(new Sequence(gInputRibozymeSequence.Length), rootNode);

            //*********************
            //4- Traverse cut site tree
            //*********************

            foreach (Node rootNode in gNodesAtDepthCutSite[0])
                TraverseNoStructure(new Sequence(gInputRibozymeCutSite.Length), rootNode);

            //*********************
            //5- Eliminate potential cut sites that are not found on input RNA
            //*********************
            EliminateCutSites();

            //*********************
            //6- Finish generating sequences based on cut sites and create list of all permutations of sequences + accepted cut sites
            //*********************

            CompleteSequencesWithCutSiteInfo();

            Console.WriteLine("Amount of accepted sequences: {0}", gSequencesToSend.Count);

            Console.WriteLine("Accepted cut sites: ");
            foreach (Sequence cutsite in gCutSiteSequences)
                Console.WriteLine(cutsite.GetString());

            Console.WriteLine("Sending sequences: ");
            foreach (Sequence seq in gSequencesToSend)
                Console.WriteLine(seq.GetString());

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
                if (inputStructure != null && !gRibozyme.mRNALinkIndices.Contains(i))
                {
                    char structure = inputStructure[i];

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
                            Console.WriteLine("Unrecognized structure symbol encountered.");
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

        static public void TraverseNoStructure(Sequence currentSequence, Node currentNode)
        {
            currentSequence.mNucleotides.Add(currentNode.mNucleotide);

            if (currentNode.mChildren.Count == 0) //Leaf
            {
                gCutSiteSequences.Add(currentSequence);
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

            if (currentNode.mChildren.Count == 0) //Leaf
            {
                if (gSequences.Contains(currentSequence))
                {
                    Console.WriteLine("Inserting duplicate!");
                }
                gSequences.Add(currentSequence);
                Console.WriteLine("{0} : {1}", currentSequence.GetString(), gSequences.Count);
            }
            else
            {
                //If child has no neighbour OR child is going to be linked to RNA, just continue
                if (gRibozyme.mRNALinkIndices.Contains(currentNode.mDepth + 1))
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
                complement.Append(c);

            return complement.ToString();
        }

        static public void EliminateCutSites()
        {
            for (int i = gCutSiteSequences.Count-1; i > -1; i--)
            {
                if (gInputRNASequence.IndexOf(gCutSiteSequences[i].GetString()) == -1)
                {
                    gCutSiteSequences.RemoveAt(i);
                }
            }
        }

        static public void GetUserInput()
        {
            FileStream filestream = new System.IO.FileStream("C:\\Users\\anita\\Desktop\\simpleRibozyme.txt",
                                          System.IO.FileMode.Open,
                                          System.IO.FileAccess.Read,
                                          System.IO.FileShare.ReadWrite);
            var file = new System.IO.StreamReader(filestream, System.Text.Encoding.UTF8, true, 128);

            gInputRibozymeSequence = file.ReadLine();
            gInputRibozymeStructure = file.ReadLine();
            gInputRibozymeCutSite = file.ReadLine();
            String RNAIndicesString = file.ReadLine();
            List<String> RNAIndices = RNAIndicesString.Split(' ').ToList();
            foreach (String s in RNAIndices)
            {
                if (s[0] == '-')
                    gInputRNALinkIndices.Add(int.MaxValue);
                else
                    gInputRNALinkIndices.Add(Convert.ToInt16(s));
            }
            gInputRNASequence = file.ReadLine();

            //Console.WriteLine("Enter ribozyme sequence: \n");
            //gInputRibozymeSequence = Console.ReadLine();

            //Console.WriteLine("Enter ribozyme secondary structure: \n");
            //gInputRibozymeStructure = Console.ReadLine();

            //Console.WriteLine("Enter cut site sequence: \n");
            //gInputRibozymeCutSite = Console.ReadLine();

            //Console.WriteLine("Enter RNA link indices: \n");
            //String RNAIndicesString = Console.ReadLine();
            //List<String> RNAIndices = RNAIndicesString.Split(' ').ToList();
            //foreach (String s in RNAIndices)
            //{
            //    if (s[0] == '-')
            //        gInputRNALinkIndices.Add(int.MaxValue);
            //    else
            //        gInputRNALinkIndices.Add(Convert.ToInt16(s));
            //}

            //Console.WriteLine("Enter RNA sequence: \n");
            //gInputRNASequence = Console.ReadLine();
        }

        static public void CompleteSequencesWithCutSiteInfo()
        {
            foreach (Sequence cutSite in gCutSiteSequences)
            {
                String complement = GetComplement(cutSite.GetString());

                foreach (Sequence ribozymeSequence in gSequences)
                {
                    Sequence newSequence = new Sequence(ribozymeSequence);
                    int index = 0;
                    foreach (int i in gRibozyme.mRNALinkIndices)
                    {
                        if (i != int.MaxValue)
                        {
                            newSequence.mNucleotides[i] = new Nucleotide(complement[index]);
                        }
                        index++;
                    }

                    gSequencesToSend.Add(newSequence);
                }
            }
        }
    }
}
