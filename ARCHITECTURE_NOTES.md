# Ribosoft Architecture Notes

## Job Processing Pipeline Issues

### Current Pipeline Problem

The current job processing pipeline has an architectural inconsistency:

```
CandidateGenerator → Structure → [Specificity] → MultiObjectiveOptimization → Completed
```

**Issues:**
1. **Inconsistent Evaluation**: Some scores (accessibility, temperature) are calculated during `CandidateGenerator`, while structure scores are calculated in a separate `Structure` phase
2. **Artificial State Separation**: Structure scoring is just another evaluation metric, not a distinct processing phase
3. **User Confusion**: Users see "Structure" as a separate phase when it's really just scoring
4. **Unnecessary Complexity**: Extra state transitions and complexity

### Proposed Solution

**Better Pipeline:**
```
CandidateGenerator → Evaluation → [Specificity] → MultiObjectiveOptimization → Completed
```

**Evaluation Phase Should Include:**
- Accessibility scoring ✅ (currently in CandidateGenerator)
- Temperature scoring ✅ (currently in CandidateGenerator)  
- Structure scoring ❌ (currently separate Structure phase)

### Implementation Plan

1. **Move structure scoring into CandidateGenerator phase**
   - Rename `CandidateGenerator` state to `Evaluation` 
   - Move `_ribosoftAlgo.Structure(designs)` call into `RunCandidateGenerator`
   - Remove separate `Structure` state and `CalculateStructure` method

2. **Update state transitions**
   - Remove `Structure` from JobState enum
   - Update `InProgress()` method
   - Update phase 2/3 conditions to check `Evaluation` instead of `Structure`

3. **Benefits**
   - Consistent evaluation in one phase
   - Simpler state machine
   - Better user experience
   - Cleaner architecture

### Why Structure Was Separated Originally

Likely reasons for the current design:
1. **Computational intensity** - Structure folding is expensive
2. **Batch processing requirement** - Needs all designs to calculate relative scores
3. **Progress tracking** - Users can see structure calculation progress
4. **Historical evolution** - May have been added later as a separate feature

### Migration Considerations

- **Database**: Existing jobs in `Structure` state need migration
- **UI**: Update progress indicators and status messages
- **Testing**: Update all tests that reference `Structure` state
- **Documentation**: Update user-facing documentation

This refactor would significantly improve the architecture while maintaining all functionality.
