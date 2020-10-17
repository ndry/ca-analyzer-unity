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
    public bool useNCell;
    // public Rule(Packer packer, int[] table) {
    //     this.packer = packer;
    //     this.table = table;
    //     code = RuleConverter.GetNumberFromDigits(table, stateCount);
    //     tableDesc = string.Join("", table);
    //     tablePacked = RuleConverter.RepackTable(
    //         table, stateCount, stateCountPacked);
    //     tablePackedNCell = RuleConverter.RepackTableFor2Cell(tablePacked,
    //         stateCountPacked);
    // }

    public Rule(Packer packer, BigInteger code, bool useNCell, int tablePackN) {
        this.packer = packer;
        this.code = code;
        this.useNCell = useNCell;
        this.tablePackN = tablePackN;
        table = GetTableFromCode(
            code, GetRuleSpaceSizePower(stateCount), stateCount);
        tableDesc = string.Join("", table);
        tablePacked = RuleConverter.RepackTable(
            table, stateCount, stateCountPacked);
        if (useNCell) {
            using (new MeasureTime { name = nameof(RuleConverter.RepackTableForNCell) + $" n = {tablePackN}" }) {
                tablePackedNCell = RuleConverter.RepackTableForNCell(
                    packer, tablePacked, tablePackN);
            }
        }
    }

    public static int GetCombinedState(int n1, int c, int n2, int pc, int stateCount) {
        var combinedState = 0;
        combinedState = combinedState * stateCount + pc;
        combinedState = combinedState * stateCount + n2;
        combinedState = combinedState * stateCount + c;
        combinedState = combinedState * stateCount + n1;
        return combinedState;
    }
    public static int GetCombinedState(int n1cn2, int pc, int stateCount)
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
        var ss = t0Space.spaceSize;
        var ps = t0Space.packedSize;

        var n = useNCell ? tablePackN : 1;
        var tableN = useNCell ? tablePackedNCell : tablePacked;

        var nCellShift = n * packer.cellSizeBits;
        var nCellMask = (1 << nCellShift) - 1;
        var np2CellShift = (n + 2) * packer.cellSizeBits;
        var np2CellMask = (1 << np2CellShift) - 1;
        var nLastCellShift = (packer.cellsPerPack - n) * packer.cellSizeBits;
        var nLastCellMask = (1 << nLastCellShift) - 1;

        var _1CellShift = 1 * packer.cellSizeBits;
        var _1CellMask = (1 << _1CellShift) - 1;
        var _2CellShift = 2 * packer.cellSizeBits;
        var _2CellMask = (1 << _2CellShift) - 1;
        var _3CellShift = 3 * packer.cellSizeBits;
        var _3CellMask = (1 << _3CellShift) - 1;
        var _1LastCellShift = (packer.cellsPerPack - 1) * packer.cellSizeBits;
        var _1LastCellMask = (1 << _1LastCellShift) - 1;

        for (var px = 0; px < ps; px++) {
            var ptm1n1 = (px - 1 >= 0) ? tm1Space.packed[px - 1] : 0;
            var ptm1c = tm1Space.packed[px];
            var ptm1n2 = (px + 1 < ps) ? tm1Space.packed[px + 1] : 0;
            var ptm2c = tm2Space.packed[px];

            int pack;
            var sx = 0;

            {
                var pc = ptm2c & _1CellMask;
                var cn2 = ptm1c & _2CellMask;
                var n1 = (ptm1n1 >> _1LastCellShift) & _1CellMask;
                var combinedState =
                    (pc << _3CellShift)
                    | (cn2 << _1CellShift)
                    | (n1);
                var state = tablePacked[combinedState];
                pack = state << _1LastCellShift;
                sx++;
            }

            var _ptm1c = ptm1c;
            var _ptm2c = ptm2c >> _1CellShift;


            while (sx < packer.cellsPerPack - n) {
                var pc1pc2 = _ptm2c & nCellMask;
                var n1c1c2n2 = _ptm1c & np2CellMask;
                var combinedState = (pc1pc2 << np2CellShift) | n1c1c2n2;
                var state = tableN[combinedState];
                pack = ((pack >> nCellShift) & nLastCellMask) | (state << nLastCellShift);
                sx += n;

                _ptm1c = _ptm1c >> nCellShift;
                _ptm2c = _ptm2c >> nCellShift;
            }

            while (sx < packer.cellsPerPack - 1) {
                var pc1pc2 = _ptm2c & _1CellMask;
                var n1c1c2n2 = _ptm1c & _3CellMask;
                var combinedState = (pc1pc2 << _3CellShift) | n1c1c2n2;
                var state = tableN[combinedState];
                pack = ((pack >> _1CellShift) & _1LastCellMask) | (state << _1LastCellShift);
                sx += 1;

                _ptm1c = _ptm1c >> _1CellShift;
                _ptm2c = _ptm2c >> _1CellShift;
            }

            {
                var pc = _ptm2c & _1CellMask;
                var n1c = _ptm1c & _2CellMask;
                var n2 = ptm1n2 & _1CellMask;
                var combinedState =
                    (pc << _3CellShift)
                    | (n2 << _2CellShift)
                    | (n1c);
                var state = tablePacked[combinedState];
                pack = ((pack >> _1CellShift) & _1LastCellMask) | (state << _1LastCellShift);
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
    public PackedSpace FillSpacePacked(IList<PackedSpace> spacetime, int t) {
        var space = spacetime[t];
        FillSpace(space, spacetime[t - 1], spacetime[t - 2]);
        return space;
    }
    public static int[] GetTableFromCode(BigInteger code, int sizePower, int stateCount) {
        var t = RuleConverter.GetDigitsFromNumber(code, stateCount);
        var padLength = sizePower - t.Length;
        return t.Concat(Enumerable.Repeat(0, padLength)).ToArray();
    }
}
