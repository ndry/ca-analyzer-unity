using UnityEngine;
public class PackedSpace : Rule.ISpace<int> {
    public Packer packer;
    public int[] packed { get; }
    public int spaceSize { get; }
    public int packedSize { get; }
    
    public PackedSpace(Packer packer, int spaceSize) {
        this.packer = packer;
        this.spaceSize = spaceSize;

        packedSize = (spaceSize - 1) / packer.cellsPerPack + 1;
        packed = new int[packedSize];
    }

    public int this[int x] {
        set => packer.SetIntoPack(
            ref packed[x / packer.cellsPerPack], (x % packer.cellsPerPack), value);
        get => packer.GetFromPack(
            packed[x / packer.cellsPerPack], (x % packer.cellsPerPack));
    }
}
