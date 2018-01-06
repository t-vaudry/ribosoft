using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ribosoft.CandidateGeneration
{
    public class CandidateGenerator
    {
        public List<Tuple<int, int>> NeighboursIndices { get; set; }

        public Ribozyme Ribozyme { get; set; }

        public List<Sequence> Sequences { get; set; }
        public List<Sequence> SubstrateSequences { get; set; }

        //Holds the index equivalencies of bonding ribozyme/substrate pairs
        public List<Tuple<int, int>> RibozymeSubstrateIndexPairs { get; set; }

        public List<Sequence> SequencesToSend { get; set; }

        public String InputRNASequence { get; set; }

        private List<List<Node>> NodesAtDepthSequence;
        private List<List<Node>> NodesAtDepthCutSite;

        private Stack<int> OpenBondIndices { get; set; }
        private Stack<int> OpenPseudoKnotIndices { get; set; }

        public CandidateGenerator()
        {
            NeighboursIndices = new List<Tuple<int, int>>();
            Sequences = new List<Sequence>();
            SubstrateSequences = new List<Sequence>();
            RibozymeSubstrateIndexPairs = new List<Tuple<int, int>>();
            SequencesToSend = new List<Sequence>();
            NodesAtDepthSequence = new List<List<Node>>();
            NodesAtDepthCutSite = new List<List<Node>>();
            OpenBondIndices = new Stack<int>();
            OpenPseudoKnotIndices = new Stack<int>();
        }

        public void GenerateCandidates(String ribozymeSeq, String ribozymeStruc, String substrateSeq, String substrateStruc, String rnaInput)
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

            GetUserInput(ribozymeSeq, ribozymeStruc, substrateSeq, substrateStruc, rnaInput);

            //*********************
            //2- Generate the tree structures
            //*********************

            GenerateStructure(NodesAtDepthSequence, Ribozyme.Sequence, Ribozyme.Structure);

            //This is making the assumption that the substrate has no bonds/pseudoknots, only empty or targets
            GenerateStructure(NodesAtDepthCutSite, Ribozyme.SubstrateSequence);

            //*********************
            //3, 4- Traverse ribozyme & substrate trees
            //*********************

            Task t1 = Task.Factory.StartNew(() => TraverseRibozyme());
            Task t2 = Task.Factory.StartNew(() => TraverseSubstrate());

            try
            {
                Task.WaitAll(t1, t2);
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }

            //*********************
            //5- Eliminate potential cut sites that are not found on input RNA
            //*********************
            EliminateCutSites();

            //*********************
            //6- Finish generating sequences based on cut sites and create list of all permutations of sequences + accepted cut sites
            //*********************

            CompleteSequencesWithCutSiteInfo();

            //Console.WriteLine("Amount of sequences (no cut site): {0}", Sequences.Count);
            //Console.WriteLine("Amount of sequences to send: {0}", SequencesToSend.Count);

            //Console.WriteLine("\nAccepted cut sites: ");
            //foreach (Sequence cutsite in SubstrateSequences)
            //    Console.WriteLine(cutsite.GetString());

            //Console.WriteLine("Sending sequences: ");
            //foreach (Sequence seq in SequencesToSend)
            //    Console.WriteLine(seq.GetString());

            //Console.ReadLine();
        }

        public void GenerateStructure(List<List<Node>> nodesAtDepth, String inputSequence, String inputStructure = null)
        {
            int depth = inputSequence.Length;

            for (int i = 0; i < depth; i++)
            {
                //List for nodes at current depth
                List<Node> depth_i = new List<Node>();

                //Get input at index (nucleotide and secondary structure)
                Nucleotide nucleotide = new Nucleotide(inputSequence[i]);

                //The neighbour (of a bond or pseudoknot)
                int? neighbourIndex = null;
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
                                OpenBondIndices.Push(i);
                                break;
                            case ')': //Close an open bond
                                neighbourIndex = OpenBondIndices.Pop();
                                NeighboursIndices.Add(Tuple.Create(i, neighbourIndex.Value));
                                break;
                            case '[': //Start a pseudoknot
                                OpenPseudoKnotIndices.Push(i);
                                break;
                            case ']': //Close a pseudoknot
                                neighbourIndex = OpenPseudoKnotIndices.Pop();
                                NeighboursIndices.Add(Tuple.Create(i, neighbourIndex.Value));
                                break;
                            default: //Should not happen
                                throw new CandidateGenerationException("Unrecognized structure symbol encountered.");
                        }
                    }
                }

                //Make a node for each possible nucleotide at the current depth (if input is not a base) and set its parents to be all nodes at the previous depth
                foreach (char baseChar in nucleotide.Bases)
                {
                    Node currentNode = new Node(new Nucleotide(baseChar), i, neighbourIndex);

                    if (i > 0)
                    {
                        currentNode.Parents = nodesAtDepth[i - 1];
                    }

                    //Add this node to the nodes at the current depth
                    depth_i.Add(currentNode);
                }

                //Go to the previous depth and set all nodes' children to nodes of current depth
                if (i > 0)
                {
                    foreach (Node node in nodesAtDepth[i - 1])
                    {
                        node.Children = depth_i;
                    }
                }

                //If we have found the second element of a neighbour pair, set the neighbour index of both nodes in pair
                if (neighbourIndex.HasValue)
                {
                    foreach (Node firstNeighbour in nodesAtDepth[neighbourIndex.Value])
                    {
                        firstNeighbour.NeighbourIndex = i;
                    }
                }

                //Add all nodes created at this depth to the structue
                nodesAtDepth.Add(depth_i);
            }

            //Validate that all open parentheses have been closed
            if (OpenBondIndices.Count != 0)
            {
                throw new CandidateGenerationException("Unclosed bond found '('. Input may be faulty.");
            }
            if (OpenPseudoKnotIndices.Count != 0)
            {
                throw new CandidateGenerationException("Unclosed pseudoknot found '{'. Input may be faulty.");
            }
        }

        public void TraverseSubstrate()
        {
            foreach (Node rootNode in NodesAtDepthCutSite[0])
                TraverseNoStructure(new Sequence(Ribozyme.SubstrateSequence.Length), rootNode);
        }
        public void TraverseRibozyme()
        {
            foreach (Node rootNode in NodesAtDepthSequence[0])
                TraverseSequence(new Sequence(Ribozyme.Sequence.Length), rootNode);
        }

        public void TraverseNoStructure(Sequence currentSequence, Node currentNode)
        {
            currentSequence.Nucleotides.Add(currentNode.Nucleotide);

            if (currentNode.Children.Count == 0) //Leaf
            {
                SubstrateSequences.Add(currentSequence);
            }
            else
            {
                foreach (Node child in currentNode.Children)
                    TraverseNoStructure(new Sequence(currentSequence), child);
            }
        }

        public void TraverseSequence(Sequence currentSequence, Node currentNode)
        {
            currentSequence.Nucleotides.Add(currentNode.Nucleotide);

            //If leaf, add sequence to list
            if (currentNode.Children.Count == 0) //Leaf
            {
                if (Sequences.Contains(currentSequence))
                {
                    Console.WriteLine("Inserting duplicate!");
                }
                Sequences.Add(currentSequence);
            }
            else
            {
                //If child is going to be linked to RNA, just continue
                if (IsTarget(Ribozyme.Structure[currentNode.Depth + 1]))
                {
                    Node child = new Node(currentNode.Children[0]);
                    child.Nucleotide.SetSymbol('-');
                    TraverseSequence(new Sequence(currentSequence), child);
                }
                //Else if the children have a neighbour (will all be the same, so just check 0) and that neighbour has already been set, we must choose 
                else if (currentNode.Children[0].NeighbourIndex.HasValue && currentNode.Children[0].NeighbourIndex.Value < currentNode.Depth + 1)
                {
                    //Get the complement of the base at the specified neighbour index in the input string
                    char requiredBase = currentSequence.Nucleotides[currentNode.Children[0].NeighbourIndex.Value].GetComplement();
                    bool found = false;
                    foreach (Node child in currentNode.Children)
                    {
                        if (child.Nucleotide.Symbol == requiredBase)
                        {
                            TraverseSequence(new Sequence(currentSequence), child);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        throw new CandidateGenerationException("Neighbours don't match!");
                    }
                }
                else
                {
                    foreach (Node child in currentNode.Children)
                        TraverseSequence(new Sequence(currentSequence), child);
                }
            }
        }
        public void EliminateCutSites()
        {
            //Eliminate the cut sites that are not found in the input RNA sequence
            for (int i = SubstrateSequences.Count - 1; i > -1; i--)
            {
                if (InputRNASequence.IndexOf(SubstrateSequences[i].GetString()) == -1)
                {
                    //Console.WriteLine("Eliminating cut site: not found in RNA sequence.");
                    SubstrateSequences.RemoveAt(i);
                }
            }
        }

        public void GetUserInput(String ribozymeSeq, String ribozymeStruc, String substrateSeq, String substrateStruc, String rnaInput)
        {
            if (ribozymeSeq.Length != ribozymeStruc.Length)
            {
                throw new CandidateGenerationException("Ribozyme sequence length does not match ribozyme structure length.");
            }
            if (substrateSeq.Length != substrateStruc.Length)
            {
                throw new CandidateGenerationException("Substrate sequence length does not match substrate structure length.");
            }

            //Create ribozyme
            Ribozyme = new Ribozyme(ribozymeSeq, ribozymeStruc, substrateSeq, substrateStruc);
            InputRNASequence = rnaInput;
        }

        public void CompleteSequencesWithCutSiteInfo()
        {
            //First, build the mapping between substrate and ribozyme
            for (int i = 0; i < Ribozyme.SubstrateStructure.Length; i++)
            {
                //If the substrate is not part of the target, continue
                if (Ribozyme.SubstrateStructure[i] == '.')
                {
                    continue;
                }

                //If it is not a '.', we are expecting it to be part of the target
                if (!IsTarget(Ribozyme.SubstrateStructure[i]))
                {
                    throw new CandidateGenerationException(String.Format("Unexpected substrate structure character: {0}", Ribozyme.SubstrateStructure[i]));
                }

                //Now find the base in the input sequence that binds to this (aka has the same struc value)
                bool foundMatch = false;
                for (int j = 0; j < Ribozyme.Structure.Length; j++)
                {
                    if (Ribozyme.Structure[j] == Ribozyme.SubstrateStructure[i])
                    {
                        foundMatch = true;
                        RibozymeSubstrateIndexPairs.Add(Tuple.Create(j, i));
                    }
                }

                if (!foundMatch)
                {
                    throw new CandidateGenerationException(String.Format("Substrate structure character not found in ribozyme structure: {0}", Ribozyme.SubstrateStructure[i]));
                }
            }

            //Complete each sequence with the complement of the substrate at the target positions
            foreach (Sequence substrate in SubstrateSequences)
            {
                String substrateComplement = substrate.GetComplement();

                foreach (Sequence ribozymeSequence in Sequences)
                {
                    Sequence newSequence = new Sequence(ribozymeSequence);
                    bool success = true;

                    //For each element in the ribozyme sequence that is part of the target area, check if it is possible to bond with the substrate
                    foreach (Tuple<int, int> indexPair in RibozymeSubstrateIndexPairs)
                    {
                        int riboIdx = indexPair.Item1;
                        int substrateIdx = indexPair.Item2;

                        Nucleotide ribozymeNucleotide = new Nucleotide(Ribozyme.Sequence[riboIdx]);
                        if (ribozymeNucleotide.Bases.Contains(substrateComplement[substrateIdx]))
                        {
                            newSequence.Nucleotides[riboIdx] = new Nucleotide(substrateComplement[substrateIdx]);
                        }
                        else
                        {
                            success = false;
                            break;
                        }
                    }

                    //If all target elements can successfully bond, add this sequence to the list
                    if (success)
                    {
                        SequencesToSend.Add(newSequence);
                    }
                    //Else, do nothing: this ribozyme sequence cannot bond with the substrate
                }
            }
        }

        public bool IsTarget(char b)
        {
            return ((b >= 'a' && b <= 'z') ||
                        (b >= 'A' && b <= 'Z') ||
                        (b >= '0' && b <= '9'));
        }
    }
}
