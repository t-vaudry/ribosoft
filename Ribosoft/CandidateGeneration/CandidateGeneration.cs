using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

using Ribosoft.Biology;

namespace Ribosoft.CandidateGeneration
{
    public struct SmallestSetSubstrateInfo
    {
        public int StartIndex;
        public int EndIndex;
        public String SubstrateBase;
    }

    public class CandidateGenerator
    {
        public SmallestSetSubstrateInfo SmallestSubstarteInfo;
        public List<Tuple<int, int>> NeighboursIndices { get; set; }

        public Ribozyme Ribozyme { get; set; }

        public List<Sequence> Sequences { get; set; }
        public List<SubstrateInfo> SubstrateInfo { get; set; }

        //Holds the index equivalencies of bonding ribozyme/substrate pairs
        public List<Tuple<int, int>> RibozymeSubstrateIndexPairs { get; set; }

        public String InputRNASequence { get; set; }

        private List<List<Node>> NodesAtDepthSequence;
        private List<List<Node>> NodesAtDepthCutSite;

        private Stack<int> OpenBondIndices { get; set; }
        private Stack<int> OpenPseudoKnotIndices { get; set; }

        private List<int> RepeatStructureSymbols { get; set; }
        private List<Tuple<int, int>> RepeatRegions { get; set; }
        private String SubstrateBaseStructure { get; set; }

        public CandidateGenerator()
        {
            NeighboursIndices = new List<Tuple<int, int>>();
            Sequences = new List<Sequence>();
            SubstrateInfo = new List<SubstrateInfo>();
            RibozymeSubstrateIndexPairs = new List<Tuple<int, int>>();
            NodesAtDepthSequence = new List<List<Node>>();
            NodesAtDepthCutSite = new List<List<Node>>();
            OpenBondIndices = new Stack<int>();
            OpenPseudoKnotIndices = new Stack<int>();
            RepeatStructureSymbols = new List<int>();
            RepeatRegions = new List<Tuple<int, int>>();
        }

        public void Clear()
        {
            NeighboursIndices.Clear();
            Sequences.Clear();
            SubstrateInfo.Clear();
            RibozymeSubstrateIndexPairs.Clear();
            NodesAtDepthSequence.Clear();
            NodesAtDepthCutSite.Clear();
            OpenBondIndices.Clear();
            OpenPseudoKnotIndices.Clear();
            RepeatStructureSymbols.Clear();
            RepeatRegions.Clear();
        }

        public IEnumerable<Candidate> GenerateCandidates(String ribozymeSeq, String ribozymeStruc, String substrateSeq, String substrateStruc, String rnaInput)
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
            //F- Foreach repeat notation:
            //  a) Add to existing valid cutsites
            //  b) Find which of these new cutsites are valid
            //  c) Complete new ribozyme with this new cutsite
            //G- Send list generated in E- to algo
            //
            //*********************

            //*********************
            //1- Get user input
            //*********************
            GetUserInput(ribozymeSeq, ribozymeStruc, substrateSeq, substrateStruc, rnaInput);

            //*********************
            //1b)- Get repeat notation info
            //*********************
            GetRepeatNotationInfo();

            //*********************
            //2- Generate the tree structures
            //*********************
            GenerateStructure(NodesAtDepthSequence, Ribozyme.Sequence, Ribozyme.Structure, true);

            //This is making the assumption that the substrate has no bonds/pseudoknots, only empty or targets
            GenerateStructure(NodesAtDepthCutSite, Ribozyme.SubstrateSequence, Ribozyme.SubstrateStructure, false);

            //*********************
            //3, 4- Traverse ribozyme & substrate trees
            //*********************
            TraverseRibozyme();
            if (GetLargestSetSubstrateRegion())
                GetSubstrates();
            else
                TraverseSubstrate();

            //*********************
            //5- Handle repeat notations at extremities
            //*********************
            HandleExtremityRepeats();

            //*********************
            //6- Finish generating sequences based on cut sites and create list of all permutations of sequences + accepted cut sites
            //*********************
            return CompleteSequencesWithCutSiteInfo();
        }

        public void GenerateStructure(List<List<Node>> nodesAtDepth, String inputSequence, String inputStructure, bool isRibozyme)
        {
            int depth = inputSequence.Length;
            int latestNonRepeatIndex = -1;

            for (int i = 0; i < depth; i++)
            {
                //Ignore repeat notations for now. They will be dealt with at the end
                if (!isRibozyme && RepeatStructureSymbols.Contains(inputStructure[i]))
                {
                    continue;
                }
                latestNonRepeatIndex++;

                //List for nodes at current depth
                List<Node> depth_i = new List<Node>();

                //Get input at index (nucleotide and secondary structure)
                Nucleotide nucleotide = new Nucleotide(inputSequence[i]);

                //The neighbour (of a bond or pseudoknot)
                int? neighbourIndex = null;
                bool isTarget = false;
                if (isRibozyme)
                {
                    char structure = inputStructure[i];
                    isTarget = IsTarget(structure);
                    //If nucleotide is NOT a target
                    if (!isTarget)
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
                //Case 1: The structure is not null (we are processing ribozyme) and the nucloetide is a target. Its base will be determined uniquely by the substrate nucleotide it bonds to
                if (isTarget && isRibozyme)
                {
                    Node currentNode = new Node(new Nucleotide(nucleotide.Symbol), i, neighbourIndex);

                    if (latestNonRepeatIndex > 0)
                    {
                        currentNode.Parents = nodesAtDepth[latestNonRepeatIndex - 1];
                    }

                    depth_i.Add(currentNode);
                }
                //Case 2: There is no input structure (we are processing substrate) or there is a structure, but this nucleotide does not bond to the substrate.
                //In either case, the base will not be determined by a bond, so develop all possibilities
                else
                {
                    foreach (char baseChar in nucleotide.Bases)
                    {
                        Node currentNode = new Node(new Nucleotide(baseChar), i, neighbourIndex);

                        if (latestNonRepeatIndex > 0)
                        {
                            currentNode.Parents = nodesAtDepth[latestNonRepeatIndex - 1];
                        }

                        //Add this node to the nodes at the current depth
                        depth_i.Add(currentNode);
                    }
                }

                //Go to the previous depth and set all nodes' children to nodes of current depth
                if (latestNonRepeatIndex > 0)
                {
                    foreach (Node node in nodesAtDepth[latestNonRepeatIndex - 1])
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
            ValidateOpenBonds();
        }

        public void TraverseSubstrate()
        {
            foreach (Node rootNode in NodesAtDepthCutSite[0])
                TraverseNoStructure(new Sequence(Ribozyme.SubstrateSequence.Length), rootNode);
        }
        public void TraverseRibozyme()
        {
            foreach (Node rootNode in NodesAtDepthSequence[0])
            {
                if (IsTarget(Ribozyme.Structure[0]))
                {
                    rootNode.Nucleotide.SetSymbol('-');
                }
                TraverseSequence(new Sequence(Ribozyme.Sequence.Length), rootNode);
            }
        }

        //Find the longest specified (A, C, G, U) portion of the substrate
        public bool GetLargestSetSubstrateRegion()
        {
            int tmpStart = -1;
            int tmpEnd = -1;

            int longestStart = -1;
            int longestEnd = -1;
            int longest = -1;

            bool started = false;

            //Only consider the portions without repeat regions
            int start = NodesAtDepthCutSite[0][0].Depth;
            int end = NodesAtDepthCutSite[NodesAtDepthCutSite.Count - 1][0].Depth;

            for(int i = start; i < end; i++)
            {
                char c = Ribozyme.SubstrateSequence[i];
                if (!started && (c == 'A' || c == 'C' || c == 'G' || c == 'U'))
                {
                    started = true;
                    tmpStart = i;
                }
                else if (started && !(c == 'A' || c == 'C' || c == 'G' || c == 'U'))
                {
                    started = false;
                    tmpEnd = i;

                    if (tmpEnd - tmpStart > longest)
                    {
                        longest = tmpEnd - tmpStart;
                        longestEnd = tmpEnd;
                        longestStart = tmpStart;
                    }
                }
            }

            bool success = longestStart != -1 && longestEnd != -1;

            if (success)
            {
                SmallestSubstarteInfo.StartIndex = longestStart;
                SmallestSubstarteInfo.EndIndex = longestEnd;
                SmallestSubstarteInfo.SubstrateBase = Ribozyme.SubstrateSequence.Substring(longestStart, longest);
            }

            return success;
        }

        //Get the substrates that are found in the RNA input
        public void GetSubstrates()
        {
            //Locations of specified substrate portion within the RNA input
            List<int> substrateIndices = AllIndicesOf(InputRNASequence, SmallestSubstarteInfo.SubstrateBase);

            //Number of characters to specify before and after the specified portion
            int lengthBefore = SmallestSubstarteInfo.StartIndex - NodesAtDepthCutSite[0][0].Depth;
            int lengthAfter = NodesAtDepthCutSite[NodesAtDepthCutSite.Count-1][0].Depth - SmallestSubstarteInfo.EndIndex + 1;

            //Total length of the specified portion
            int length = SmallestSubstarteInfo.EndIndex - SmallestSubstarteInfo.StartIndex;

            //Loop through all spots in the RNA input where the specified portion of the substrate was found
            //Complete the unspecified part of the substrate using the RNA input
            foreach (int idxInRNA in substrateIndices)
            {
                Sequence sequence = new Sequence();

                //If the index in the RNA is such that the substrate won't fit around it, ignore it
                if (idxInRNA < lengthBefore || (InputRNASequence.Length - idxInRNA) < lengthAfter + length)
                    continue;

                bool found = true;
                //Add the elements before the specified portion
                for (int i = lengthBefore; i > 0; i--)
                {
                    Nucleotide substrate = new Nucleotide(Ribozyme.SubstrateSequence[SmallestSubstarteInfo.StartIndex - i]);

                    if (substrate.Bases.Contains(InputRNASequence[idxInRNA - i]))
                    {
                        sequence.Nucleotides.Add(new Nucleotide(InputRNASequence[idxInRNA - i]));
                    }
                    else
                    {
                        found = false;
                        break;
                    }
                }

                if (!found)
                {
                    continue;
                }

                //Add the specified portion
                foreach (char c in SmallestSubstarteInfo.SubstrateBase)
                {
                    sequence.Nucleotides.Add(new Nucleotide(c));
                }

                //Add the elements after the specified portion
                for (int i = 0; i < lengthAfter; i++)
                {
                    Nucleotide substrate = new Nucleotide(Ribozyme.SubstrateSequence[SmallestSubstarteInfo.EndIndex + i]);

                    if (substrate.Bases.Contains(InputRNASequence[idxInRNA + length + i]))
                    {
                        sequence.Nucleotides.Add(new Nucleotide(InputRNASequence[idxInRNA + length + i]));
                    }
                    else
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    //Add unique, since it is possible to build the same substrate multiple times from the RNA
                    SubstrateInfo substrateInfo = new SubstrateInfo(sequence, SubstrateBaseStructure);
                    if (!SubstrateInfo.Contains(substrateInfo))
                        SubstrateInfo.Add(substrateInfo);
                }
            }
        }

        public void TraverseNoStructure(Sequence currentSequence, Node currentNode)
        {
            currentSequence.Nucleotides.Add(currentNode.Nucleotide);

            if (currentNode.Children.Count == 0) //Leaf
            {
                //Eliminate the cut sites that are not found in the input RNA sequence
                if (InputRNASequence.IndexOf(currentSequence.GetString(), StringComparison.Ordinal) >= 0)
                {
                    //Console.WriteLine("Eliminating cut site: not found in RNA sequence.");
                    SubstrateInfo.Add(new SubstrateInfo(currentSequence, SubstrateBaseStructure));
                }
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
                if (!Sequences.Contains(currentSequence))
                {
                    Sequences.Add(currentSequence);
                }
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
                    char[] requiredBases = currentSequence.Nucleotides[currentNode.Children[0].NeighbourIndex.Value].GetSpecialComplements();
                    bool found = false;
                    foreach (char c in requiredBases)
                    {
                        foreach (Node child in currentNode.Children)
                        {
                            if (child.Nucleotide.Symbol == c)
                            {
                                TraverseSequence(new Sequence(currentSequence), child);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            //If not found, there is the possibility that one of the neighbour's siblings is valid.
                            //In this case, there is no error: silently discard this sequence
                            Nucleotide inputNeighbourNucleotide = new Nucleotide(Ribozyme.Sequence[currentNode.Children[0].NeighbourIndex.Value]);
                            if (inputNeighbourNucleotide.Bases.Contains(c))
                            {
                                return;
                            }
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

        public void GetRepeatNotationInfo()
        {
            StringBuilder baseStructure = new System.Text.StringBuilder();

            bool previousIsRepeat = false;
            int currentRegionStartIndex = -1;

            //Check if any of the inputs are lowercase (repeat notation)
            for (int i = 0; i < Ribozyme.SubstrateSequence.Length; i++)
            {
                if (char.IsLower(Ribozyme.SubstrateSequence[i]))
                {
                    char structureSymbol = Ribozyme.SubstrateStructure[i];
                    if (!IsTarget(structureSymbol))
                    {
                        throw new CandidateGeneration.CandidateGenerationException("Repeat notation is not on target. Unhandled case.");
                    }

                    RepeatStructureSymbols.Add(structureSymbol);

                    if (!previousIsRepeat)
                    {
                        //Start new repeat region
                        currentRegionStartIndex = i;
                    }

                    previousIsRepeat = true;
                }
                else
                {
                    baseStructure.Append(Ribozyme.SubstrateStructure[i]);

                    if (previousIsRepeat)
                    {
                        //End repeat region
                        RepeatRegions.Add(Tuple.Create(currentRegionStartIndex, i - 1));
                        previousIsRepeat = false;
                    }
                }
            }

            //Close the last repeat region, if any
            if (previousIsRepeat)
            {
                RepeatRegions.Add(Tuple.Create(currentRegionStartIndex, Ribozyme.SubstrateSequence.Length - 1));
            }

            //Check if multiple repeat regions. This case isn't handled yet
            if (RepeatRegions.Count > 2)
            {
                throw new CandidateGenerationException("More than 2 repeat regions are not supported.");
            }

            //Check if any repeat regions aren't at either extremity. This case isn't handled yet.
            foreach(Tuple<int,int> repeatRegion in RepeatRegions)
            {
                //If start at the beginning, good
                if (repeatRegion.Item1 == 0)
                {
                    continue;
                }

                //If end at the end, good
                if (repeatRegion.Item2 == Ribozyme.SubstrateSequence.Length-1)
                {
                    continue;
                }

                //The repeat region is in the middle of the sequence. We don't handle this right now.
                throw new CandidateGenerationException("Repeat notation not located at beginning or end of substrate sequence. Case not supported.");
            }

            SubstrateBaseStructure = baseStructure.ToString();
        }

        public IEnumerable<Candidate> CompleteSequencesWithCutSiteInfo()
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
            foreach (SubstrateInfo substrateInfo in SubstrateInfo)
            {
                String substrateComplement = substrateInfo.Sequence.GetComplement();

                foreach (Sequence ribozymeSequence in Sequences)
                {
                    Sequence newSequence = new Sequence(ribozymeSequence);
                    bool success = true;

                    //For each element in the ribozyme sequence that is part of the target area, check if it is possible to bond with the substrate
                    foreach (Tuple<int, int> indexPair in RibozymeSubstrateIndexPairs)
                    {
                        int riboIdx = indexPair.Item1;
                        int substrateIdx = indexPair.Item2;

                        //Check if this substrate sequence has this bond (may not due to repeat notation)
                        char bondID = Ribozyme.SubstrateStructure[substrateIdx];
                        substrateIdx = substrateInfo.Structure.IndexOf(bondID);
                        if (substrateIdx == -1)
                        {
                            continue;
                        }

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
                        //Remove the nucleotides that are not part of the sequence (due to repeat notation)
                        String newStructure = Ribozyme.Structure;
                        RemoveUnusedRepeats(ref newStructure, newSequence);
                        yield return new Candidate { Sequence = newSequence, Structure = newStructure, SubstrateSequence = substrateInfo.Sequence.GetString(), SubstrateStructure = substrateInfo.Structure, CutsiteNumberOffset = substrateInfo.CutsiteOffset, CutsiteIndices = AllIndicesOf(InputRNASequence, substrateInfo.Sequence.GetString()) };
                    }
                    //Else, do nothing: this ribozyme sequence cannot bond with the substrate
                }
            }
        }

        private void RemoveUnusedRepeats(ref String structure, Sequence sequence)
        {
            for (int i = sequence.GetLength() - 1; i > -1; i--)
            {
                if (sequence.Nucleotides[i].Symbol == '-')
                {
                    sequence.Nucleotides.RemoveAt(i);
                    structure = structure.Remove(i, 1);
                }
            }
        }

        public void HandleExtremityRepeats()
        {
            foreach (Tuple<int, int> repeatRegion in RepeatRegions)
            {
                //Always base off of the current list
                //If this isn't the first repeat region, this will base off of the no repeat versions as well as all the ones with the other repeat region
                IList<SubstrateInfo> sequencesToBaseOffOf = new List<SubstrateInfo>(SubstrateInfo);

                int currentCount = 0;

                IList<SubstrateInfo> newSubstrateSequences = new List<SubstrateInfo>();

                bool startRepeat = (repeatRegion.Item1 == 0);
                bool endRepeat = (repeatRegion.Item2 == Ribozyme.SubstrateSequence.Length - 1);

                while (currentCount <= (repeatRegion.Item2 - repeatRegion.Item1))
                {
                    int beginIdx = -1;

                    if (!startRepeat && !endRepeat)
                    {
                        throw new CandidateGenerationException("Repeat notation not located at beginning or end of substrate sequence. Case not supported.");
                    }

                    beginIdx = startRepeat ? (repeatRegion.Item2 - currentCount) : (repeatRegion.Item1 + currentCount);

                    Nucleotide newBeginning = new Nucleotide(Ribozyme.SubstrateSequence[beginIdx]);
                    char additionalStructure = Ribozyme.SubstrateStructure[beginIdx];

                    foreach (char baseSymbol in newBeginning.Bases)
                    {
                        foreach (SubstrateInfo seq in sequencesToBaseOffOf)
                        {
                            Sequence newSeq = new Sequence(seq.Sequence);
                            int insertIdx = startRepeat ? 0 : newSeq.GetLength();
                            newSeq.Insert(insertIdx, new Nucleotide(baseSymbol));
                            String newStructure = startRepeat ? additionalStructure + seq.Structure : seq.Structure + additionalStructure;

                            int offset = startRepeat ? (currentCount + 1) : 0; //If there is a repeat at the beginning, the cutsite number should be shifted

                            //Keep only cutsites that are found in RNA
                            if (InputRNASequence.IndexOf(newSeq.GetString(), StringComparison.Ordinal) >= 0)
                            {
                                newSubstrateSequences.Add(new SubstrateInfo(newSeq, newStructure, offset));
                            }
                        }
                    }

                    //Add valid cutsites to list
                    SubstrateInfo.AddRange(newSubstrateSequences);

                    //Next added repeat will base itself off of this new list
                    sequencesToBaseOffOf = new List<SubstrateInfo>(newSubstrateSequences);
                    newSubstrateSequences.Clear();

                    currentCount++;
                }
            }
        }

        private void ValidateOpenBonds()
        {
            if (OpenBondIndices.Count != 0)
            {
                throw new CandidateGenerationException("Unclosed bond found '('. Input may be faulty.");
            }
            if (OpenPseudoKnotIndices.Count != 0)
            {
                throw new CandidateGenerationException("Unclosed pseudoknot found '{'. Input may be faulty.");
            }
        }

        public bool IsTarget(char b)
        {
            return ((b >= 'a' && b <= 'z') ||
                        (b >= 'A' && b <= 'Z') ||
                        (b >= '0' && b <= '9'));
        }

        public static List<int> AllIndicesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException("the string to find may not be empty", "value");
            }

            List<int> indices = new List<int>();
            for (int index = 0;; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                {
                    return indices;
                }
                    
                indices.Add(index);
            }
        }
    }
}
