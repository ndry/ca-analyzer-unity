using UnityEngine;
public class PackedSpace : Rule.ISpace<int> {
    public Packer packer;
    public int[] packed { get; }
    public int spaceSize { get; }
    public int packedSize { get; }

    public PackedSpace(Packer packer, int spaceSize) {
        this.packer = packer;
        this.spaceSize = spaceSize;

        packedSize = packer.GetPackedSize(spaceSize);
        packed = new int[packedSize];
    }
    public PackedSpace(Packer packer, int[] space) : this(packer, space.Length) {
        for (var i = 0; i < space.Length; i++) {
            this[i] = space[i];
        }
    }

    public int this[int x] {
        set => packer.SetIntoPack(
            ref packed[x / packer.cellsPerPack], (x % packer.cellsPerPack), value);
        get => packer.GetFromPack(
            packed[x / packer.cellsPerPack], (x % packer.cellsPerPack));
    }
}

public static class PackedSpaceEx {
    public static int[] Convolute(this PackedSpace space) {
        var packer = space.packer;
        var convolution = new int[space.spaceSize];
        var x = 0;
        void ConvolutePack(int pack, int nextPack, bool isLast = false) {
            convolution[x++] = pack;
            if (isLast & x >= convolution.Length) { return; }
            for (var sx = 1; sx < packer.cellsPerPack; sx++) {

                var shift1 = sx * packer.cellSizeBits;
                var shift2 = Packer.sizeOfPackBits - shift1;
                var mask2 = ~(-1 << shift2);

                var conv = (nextPack << shift2) | ((pack >> shift1) & mask2);

                convolution[x++] = conv;
                if (isLast & x >= convolution.Length) { return; }
            }
        }
        var nextPack = (0 < space.packedSize) ? space.packed[0] : 0;
        for (var px = 1; px < space.packedSize; px++) {
            var pack = nextPack;
            nextPack = space.packed[px];
            ConvolutePack(pack, nextPack);
        }
        ConvolutePack(nextPack, 0, true);
        // Debug.Log(string.Join(",", convolution));
        return convolution;
    }
}
