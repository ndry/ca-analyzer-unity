using UnityEngine;

public class Packer {
    public static readonly int sizeOfPack = sizeof(int);
    public static readonly int sizeOfPackBits = sizeOfPack * 8;
    public readonly int stateCount;
    public readonly int stateCountPacked;
    public readonly int cellSizeBits;
    public readonly int cellsPerPack;
    public readonly int cellMask;
    public Packer(int stateCount) {
        this.stateCount = stateCount;
        stateCountPacked = Mathf.NextPowerOfTwo(stateCount);
        cellSizeBits = 1;
        while ((stateCountPacked >> cellSizeBits) > 1) { cellSizeBits++; }
        cellsPerPack = sizeOfPackBits / cellSizeBits;
        cellMask = (1 << cellSizeBits) - 1;
    }
    public void SetIntoPack(ref int pack, int i, int value) {
        var shift = i * cellSizeBits;
        pack = pack & ~(cellMask << shift) | value << shift;
    }
    public void SetIntoPackDirty(ref int pack, int i, int value) {
        var shift = i * cellSizeBits;
        pack = pack | value << shift;
    }
    public int GetFromPack(int pack, int offset) {
        var shift = offset * cellSizeBits;
        return (pack >> shift) & cellMask;
    }
    public int GetFromPack(int pack, int offset, int count) {
        var shift = offset * cellSizeBits;
        var mask = (1 << (count * cellSizeBits)) - 1;
        return (pack >> shift) & mask;
    }
    public int AppendToPack(int pack, int data, int dataCellCount) {
        var shift = dataCellCount * cellSizeBits;
        var lastShift = (cellsPerPack - dataCellCount) * cellSizeBits;
        var lastMask = (1 << lastShift) - 1;

        return ((pack >> shift) & lastMask) | (data << lastShift);
    }
    public int GetPackedSize(int spaceSize)
        => (spaceSize - 1) / cellsPerPack + 1;
}
