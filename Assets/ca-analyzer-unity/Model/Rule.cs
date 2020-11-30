using System.Collections.Generic;
using System.Linq;
using BigInteger = System.Numerics.BigInteger;
using UnityEngine;
using System;

public class Rule {
    public Packer packer;

    public interface ISpace<T> {
        T this[int index] { get; set; }
        int spaceSize { get; }
    }

    public static int GetRuleSpaceSizePower(int stateCount)
        => Mathf.RoundToInt(Mathf.Pow(stateCount, 4));
    public static BigInteger GetRuleSpaceSize(int stateCount)
        => BigInteger.Pow(stateCount, GetRuleSpaceSizePower(stateCount));


    public static readonly int spaceNeighbourhoodRadius = 1;
    public static readonly int timeNeighbourhoodRadius = 2;
    public readonly int[] table;
    public readonly int[] tablePacked;
    public readonly int[] tablePackedNCell;
    public readonly BigInteger code;
    public readonly string tableDesc;
    public int stateCount => packer.stateCount;
    public int stateCountPacked => packer.stateCountPacked;
    public int tablePackN;
    private Rule(
        Packer packer,
        BigInteger code,
        int[] table,
        int tablePackN = 1
    ) {
        this.packer = packer;
        this.code = code;
        this.table = table;
        this.tablePackN = tablePackN;
        tableDesc = string.Join("", table);
        tablePacked = RuleConverterLib.RepackTable(
            table, stateCount, stateCountPacked);
        if (tablePackN != 1) {
            tablePackedNCell = RuleConverterLib.RepackTableForNCell(
                packer, tablePacked, tablePackN);
        } else {
            tablePackedNCell = tablePacked;
        }
    }

    public Rule(
        Packer packer,
        int[] table,
        int tablePackN = 1
    ) : this(
        packer,
        RuleConverterLib.GetNumberFromDigits(table, packer.stateCount),
        table,
        tablePackN
    ) {
    }

    public Rule(
        int stateCount,
        int[] table,
        int tablePackN = 1
    ) : this(
        new Packer(stateCount),
        table,
        tablePackN
    ) {
    }

    public Rule(
        Packer packer,
        BigInteger code,
        int tablePackN = 1
    ) : this(
        packer,
        code,
        GetTableFromCode(code, GetRuleSpaceSizePower(packer.stateCount), packer.stateCount),
        tablePackN
    ) {
    }

    public static int GetCombinedState(int n1, int c, int n2, int pc, int stateCount) {
        var combinedState = 0;
        combinedState = combinedState * stateCount + pc;
        combinedState = combinedState * stateCount + n2;
        combinedState = combinedState * stateCount + c;
        combinedState = combinedState * stateCount + n1;
        return combinedState;
    }
    public static int GetCombinedStatePacked(int n1cn2, int pc, int stateCount)
        => pc * stateCount * stateCount * stateCount + n1cn2;

    public static int GetCombinedState2Cell(int n1, int c1, int c2, int n2, int pc1, int pc2, int stateCount) {
        var combinedState = 0;
        combinedState = combinedState * stateCount + pc2;
        combinedState = combinedState * stateCount + pc1;
        combinedState = combinedState * stateCount + n2;
        combinedState = combinedState * stateCount + c2;
        combinedState = combinedState * stateCount + c1;
        combinedState = combinedState * stateCount + n1;
        return combinedState;
    }

    public void FillSpace(
        PackedSpace t0Space,
        PackedSpace tm1Space,
        PackedSpace tm2Space
    ) {
        var cellSizeBits = packer.cellSizeBits;
        var cellsPerPack = packer.cellsPerPack;

        var ss = t0Space.spaceSize;
        var ps = t0Space.packedSize;

        var n = tablePackN;
        var tableN = tablePackedNCell;

        var nCellShift = n * cellSizeBits;
        var np2CellShift = (n + 2) * cellSizeBits;
        var np2CellMask = (1 << np2CellShift) - 1;

        var indexSize = n + n + 2;
        var combinedStateShift = indexSize * cellSizeBits;
        var combinedStateMask = ~(-1 << combinedStateShift);

        int GetStateNCell(int _pcn, int _n1cnn2) {
            var combinedState =
                ((_pcn << np2CellShift) | (_n1cnn2 & np2CellMask))
                    & combinedStateMask;
            return tableN[combinedState];
        }

        for (var px = 0; px < ps; px++) {
            var ptm1n1 = (px - 1 >= 0) ? tm1Space.packed[px - 1] : 0;
            var ptm1c = tm1Space.packed[px];
            var ptm1n2 = (px + 1 < ps) ? tm1Space.packed[px + 1] : 0;
            var ptm2c = tm2Space.packed[px];

            int pack = 0;

            var _ptm1c = packer.AppendToPack(ptm1n1, ptm1c, cellsPerPack - 1);

            for (var sx = 0; sx < cellsPerPack - 2; sx += n) {
                var shift = sx * cellSizeBits;
                var state = GetStateNCell(ptm2c >> shift, _ptm1c >> shift);
                pack |= state << shift;
            }

            _ptm1c = packer.AppendToPack(ptm1c, ptm1n2, 1);
            pack = pack & ~(-1 << ((cellsPerPack - 2) * cellSizeBits));

            for (var sx = cellsPerPack - 2; sx < cellsPerPack; sx += n) {
                var shift = sx * cellSizeBits;
                var state = GetStateNCell(
                    ptm2c >> shift, _ptm1c >> (shift - 2 * cellSizeBits));
                pack |= state << shift;
            }

            t0Space.packed[px] = pack;
        }
    }

    public void FillSpace(
        ISpace<int> space,
        ISpace<int> prevSpace,
        ISpace<int> prevPrevSpace
    ) {
        var nr = spaceNeighbourhoodRadius;
        var ss = space.spaceSize;

        var n1 = 0;
        var c = prevSpace[nr - 1];
        var n2 = prevSpace[nr];

        for (var x = nr; x < ss - nr; x++) {
            n1 = c;
            c = n2;
            n2 = prevSpace[x + 1];
            var pc = prevPrevSpace[x];

            var combinedState = 0;
            combinedState = combinedState * stateCount + pc;
            combinedState = combinedState * stateCount + n2;
            combinedState = combinedState * stateCount + c;
            combinedState = combinedState * stateCount + n1;

            space[x] = table[combinedState];
        }
    }
    public T FillSpace<T>(IList<T> spacetime, int t
    ) where T : ISpace<int> {
        var space = spacetime[t];
        FillSpace(space, spacetime[t - 1], spacetime[t - 2]);
        return space;
    }
    public PackedSpace FillSpace(IList<PackedSpace> spacetime, int t) {
        var space = spacetime[t];
        FillSpace(space, spacetime[t - 1], spacetime[t - 2]);
        return space;
    }
    public static int[] GetTableFromCode(BigInteger code, int sizePower, int stateCount) {
        var t = RuleConverterLib.GetDigitsFromNumber(code, stateCount);
        var padLength = sizePower - t.Length;
        return t.Concat(Enumerable.Repeat(0, padLength)).ToArray();
    }
}
