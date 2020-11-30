public class SymmetricalRuleSpace : RuleSpaceBase {
    public static int[,] symmetryMap = {
        {0, 1, 3, 6, 10},
        {1, 2, 4, 7, 11},
        {3, 4, 5, 8, 12},
        {6, 7, 8, 9, 13},
        {10, 11, 12, 13, 14},
    };

    public int symmetryStateCount =>
        1 + symmetryMap[stateCount - 1, stateCount - 1];
    public override int sizePower =>
        stateCount * symmetryStateCount * stateCount;
    public override int GetCombinedState(int n1, int c, int n2, int pc, int _) {
        var combinedState = 0;
        combinedState = combinedState * stateCount + pc;
        combinedState = combinedState * stateCount + symmetryMap[n1, n2];
        combinedState = combinedState * stateCount + c;
        return combinedState;
    }
}
