using System;
public enum RuleSpaceDesc {
    Full,
    LegacyFull,
    Totalistic,
    Symmetrical
}
public static class RuleSpacesDescEx {
    public static RuleSpaceBase Create(
        this RuleSpaceDesc ruleSpace,
        int stateCount
    ) {
        switch (ruleSpace) {
            case RuleSpaceDesc.Full:
                return new FullRuleSpace { stateCount = stateCount };
            case RuleSpaceDesc.LegacyFull:
                return new LegacyFullRuleSpace { stateCount = stateCount };
            case RuleSpaceDesc.Totalistic:
                return new TotalisticRuleSpace { stateCount = stateCount };
            case RuleSpaceDesc.Symmetrical:
                return new SymmetricalRuleSpace { stateCount = stateCount };
        }
        throw new NotSupportedException();
    }
}