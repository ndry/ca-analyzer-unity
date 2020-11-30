public class LegacyFullRuleSpace : RuleSpaceBase {
    public override int sizePower => Rule.GetRuleSpaceSizePower(stateCount);
    public override int GetCombinedState(int n1, int c, int n2, int pc, int __) {
        var combinedState = 0;
        combinedState = combinedState * stateCount + n1;
        combinedState = combinedState * stateCount + c;
        combinedState = combinedState * stateCount + n2;
        combinedState = combinedState * stateCount + pc;
        return combinedState;
    }
}
