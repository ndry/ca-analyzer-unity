public class TotalisticRuleSpace : RuleSpaceBase {
    public override int sizePower => (stateCount - 1) * 3 + 1;
    public override int GetCombinedState(int n1, int c, int n2, int pc, int __)
        => n1 + c + n2;
}
