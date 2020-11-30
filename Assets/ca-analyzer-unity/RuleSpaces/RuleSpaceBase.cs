using System.Linq;
using UnityEngine;
using BigInteger = System.Numerics.BigInteger;

public abstract class RuleSpaceBase {
    public int stateCount;
    public abstract int sizePower { get; }
    public BigInteger GetNumberFromDigits(int[] digits)
        => RuleConverterLib.GetNumberFromDigits(digits, stateCount);
    public abstract int GetCombinedState(int n1, int c, int n2, int pc, int _);
    public virtual int[] GetFullTable(int[] table)
        => RuleConverterLib.ConvertTable(table, GetCombinedState, stateCount);
    public virtual int[] GetFullTable(BigInteger code)
        => GetFullTable(Rule.GetTableFromCode(code, sizePower, stateCount));
    public virtual int[] GetFullTable(string code)
        => GetFullTable(BigInteger.Parse(code));
    public virtual BigInteger GetFullCode(BigInteger code)
        => GetNumberFromDigits(GetFullTable(code));
    public virtual string GetFullCode(string code)
        => GetFullCode(BigInteger.Parse(code)).ToString();
    public int[] GenerateRandomTable()
        => Enumerable.Range(0, sizePower)
                .Select(_ => Random.Range(0, stateCount))
                .ToArray();
    public BigInteger GenerateRandomCode()
        => GetNumberFromDigits(GenerateRandomTable());
    public string GenerateRandomCodeString()
        => GenerateRandomCode().ToString();
    public int[] GenerateRandomFullTable()
        => GetFullTable(GenerateRandomTable());
}