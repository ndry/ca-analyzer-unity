using BigInteger = System.Numerics.BigInteger;

public class FullRuleSpace : RuleSpaceBase {
    public override int sizePower => Rule.GetRuleSpaceSizePower(stateCount);
    public override int GetCombinedState(int n1, int c, int n2, int pc, int _) {
        var combinedState = 0;
        combinedState = combinedState * stateCount + pc;
        combinedState = combinedState * stateCount + n2;
        combinedState = combinedState * stateCount + c;
        combinedState = combinedState * stateCount + n1;
        return combinedState;
    }
    public override int[] GetFullTable(int[] table) => table;
    public override BigInteger GetFullCode(BigInteger code) => code;
    public override string GetFullCode(string code) => code;
}
