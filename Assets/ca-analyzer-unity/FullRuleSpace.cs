using System.Linq;
using BigInteger = System.Numerics.BigInteger;
using UnityEngine;

class FullRuleSpace {
    public static BigInteger GenerateRandomRuleCode(int stateCount) {
        var sizePower = Rule.GetRuleSpaceSizePower(stateCount);
        var table =
            Enumerable.Range(0, sizePower)
                .Select(_ => Random.Range(0, stateCount))
                .ToArray();
        return RuleConverter.GetNumberFromDigits(table, stateCount);
    }
    public Packer packer;
    public FullRuleSpace(Packer packer) {
        this.packer = packer;
    }

    public int sizePower => Rule.GetRuleSpaceSizePower(packer.stateCount);
    public BigInteger size => Rule.GetRuleSpaceSize(packer.stateCount);

    public BigInteger GenerateRandomRuleCode() => GenerateRandomRuleCode(packer.stateCount);
}
