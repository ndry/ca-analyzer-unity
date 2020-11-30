using System.Collections.Generic;
using BigInteger = System.Numerics.BigInteger;
using System;
using UnityEngine;

public static class RuleConverterLib {
    public static BigInteger GetNumberFromDigits(IReadOnlyList<int> digits, int stateCount) {
        var sum = BigInteger.Zero;
        var placeFactor = BigInteger.One;
        for (var i = 0; i < digits.Count; i++) {
            sum += digits[i] * placeFactor;
            placeFactor *= stateCount;
        }
        return sum;
    }

    public static int[] GetDigitsFromNumber(BigInteger n, int stateCount) {
        var digits = new List<int>();
        while (n != 0) {
            n = BigInteger.DivRem(n, stateCount, out var digit);
            digits.Add((int)digit);
        }

        return digits.ToArray();
    }
    public static IEnumerable<(int n1, int c, int n2, int pc)> IterateStates(int stateCount) {
        for (var n1 = 0; n1 < stateCount; n1++) {
            for (var n2 = 0; n2 < stateCount; n2++) {
                for (var c = 0; c < stateCount; c++) {
                    for (var pc = 0; pc < stateCount; pc++) {
                        yield return (n1, c, n2, pc);
                    }
                }
            }
        }
    }
    public static int[] ConvertTable(
        int[] customTable,
        Func<int, int, int, int, int, int> getCombinedStateCustom,
        int stateCount
    ) {
        var table = new int[Rule.GetRuleSpaceSizePower(stateCount)];

        foreach (var (n1, c, n2, pc) in IterateStates(stateCount)) {
            table[Rule.GetCombinedState(n1, c, n2, pc, stateCount)] =
                customTable[getCombinedStateCustom(n1, c, n2, pc, stateCount)];
        }

        return table;
    }
    public static int[] RepackTable(int[] table, int stateCountCurrent, int stateCountTarget)
    => ConvertTable(
        table,
        (n1, c, n2, pc, _) => {
            if (
                n1 >= stateCountCurrent
                || c >= stateCountCurrent
                || n2 >= stateCountCurrent
                || pc >= stateCountCurrent
            ) {
                return 0;
            }
            return Rule.GetCombinedState(n1, c, n2, pc, stateCountCurrent);
        },
        stateCountTarget);

    public static int[] RepackTableFor2Cell(int[] table, int stateCount) {
        var sizePower = Mathf.RoundToInt(Mathf.Pow(stateCount, 4 + 2));
        var t2 = new int[sizePower];
        for (var pc1 = 0; pc1 < stateCount; pc1++) {
            for (var pc2 = 0; pc2 < stateCount; pc2++) {
                for (var n1 = 0; n1 < stateCount; n1++) {
                    for (var c1 = 0; c1 < stateCount; c1++) {
                        for (var c2 = 0; c2 < stateCount; c2++) {
                            for (var n2 = 0; n2 < stateCount; n2++) {
                                var stc1 = Rule.GetCombinedState(n1, c1, c2, pc1, stateCount);
                                var stc2 = Rule.GetCombinedState(c1, c2, n2, pc2, stateCount);
                                var stc = Rule.GetCombinedState2Cell(n1, c1, c2, n2, pc1, pc2, stateCount);
                                t2[stc] = table[stc2] * stateCount + table[stc1];
                            }
                        }
                    }
                }
            }
        }
        return t2;
    }
    public static int[] RepackTableForNCell(Packer packer, int[] table, int n) {
        var stateCount = packer.stateCountPacked;
        var cellCount = (2 + n) + n;
        Debug.Assert(cellCount <= packer.cellsPerPack, "Table index exceeds 1 pack");
        var sizePower = Mathf.RoundToInt(Mathf.Pow(stateCount, cellCount));
        var t2 = new int[sizePower];
        for (var stc = 0; stc < t2.Length; stc++) {
            var st = 0;
            for (var i = 0; i < n; i++) {
                var pc = packer.GetFromPack(stc, n + 2 + i);
                var n1cn2 = packer.GetFromPack(stc, i, 3);
                var stci = Rule.GetCombinedStatePacked(n1cn2, pc, stateCount);
                var sti = table[stci];
                packer.SetIntoPackDirty(ref st, i, sti);
            }
            t2[stc] = st;
        }
        return t2;
    }
}