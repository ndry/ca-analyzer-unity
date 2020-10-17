using System.Collections;
using BigInteger = System.Numerics.BigInteger;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using System;
using Random = System.Random;

public class RuleSpacetime : MonoBehaviour {
    public readonly int stateCount = 3;
    public enum StartFill {
        Zeros, Random
    }
    public enum BorderFill {
        Cycle, Zeros, Random
    }
    public string ruleCode;
    public string otherRuleCode;
    public bool useWolframTotalistic;
    public bool convertLegacy;
    public int timeSize = 800;
    public int spaceSize = 400;
    public StartFill startFill;
    public BorderFill borderFill;
    public bool keepAspectRatio = true;
    private PackedSpace[] spacetime;
    private int[][] spacetimeConvolution;
    public bool usePackedFillSpace;

    void OnValidate() {
        if (keepAspectRatio) {
            transform.localScale = new Vector3(
                transform.localScale.z * timeSize / spaceSize,
                transform.localScale.y,
                transform.localScale.z);
        }
        if (useWolframTotalistic) {
            ruleCode = RuleConverter.GetNumberFromDigits(
                RuleConverter.GetTableFromTotalistic(
                    BigInteger.Parse(otherRuleCode),
                    stateCount),
                stateCount
            ).ToString();
        } else if (convertLegacy) {
            ruleCode = RuleConverter.GetNumberFromDigits(
                RuleConverter.GetTableFromLegacy(
                    BigInteger.Parse(otherRuleCode),
                    stateCount),
                stateCount
            ).ToString();
        }
    }
    public int patternLength = 10;
    public int cellSizeBits = 2;
    public int seed = 4242;
    public bool useSeed = true;
    public int tablePackN = 2;
    public bool useNCell = true;

    [Button]
    public void RandomizeRule() {
        ruleCode = FullRuleSpace.GenerateRandomRuleCode(stateCount).ToString();
    }

    [Button]
    public void Emulate() {
        using (var measureTime = new MeasureTime()) {

            var random = useSeed ? new Random(seed) : new Random();

            var packer = new Packer(stateCount);
            var rule = new Rule(packer, BigInteger.Parse(ruleCode), useNCell, tablePackN);

            measureTime.Mark("After rule inst");
            if (spacetime == null || spacetime.Length != timeSize || spacetime[0].spaceSize != spaceSize) {
                spacetime = new PackedSpace[timeSize];
                for (var t = 0; t < spacetime.Length; t++) {
                    spacetime[t] = new PackedSpace(packer, spaceSize);
                }
            }

            measureTime.Mark("After raw spacetime creation");
            for (var x = 0; x < spaceSize; x++) {
                switch (startFill) {
                    case StartFill.Zeros:
                        spacetime[0][x] = 0;
                        spacetime[1][x] = 0;
                        break;
                    case StartFill.Random:
                        spacetime[0][x] = random.Next(stateCount);
                        spacetime[1][x] = random.Next(stateCount);
                        break;
                }
            }
            for (var t = 2; t < spacetime.Length; t++) {
                var space =
                    usePackedFillSpace
                        ? rule.FillSpacePacked(spacetime, t)
                        : rule.FillSpace(spacetime, t);
                switch (borderFill) {
                    case BorderFill.Cycle:
                        space[0] = space[spaceSize - 4];
                        space[1] = space[spaceSize - 3];
                        space[spaceSize - 2] = space[2];
                        space[spaceSize - 1] = space[3];
                        break;
                    case BorderFill.Zeros:
                        space[0] = 0;
                        space[1] = 0;
                        space[spaceSize - 2] = 0;
                        space[spaceSize - 1] = 0;
                        break;
                    case BorderFill.Random:
                        space[0] = random.Next(stateCount);
                        space[1] = random.Next(stateCount);
                        space[spaceSize - 2] = random.Next(stateCount);
                        space[spaceSize - 1] = random.Next(stateCount);
                        break;
                }
            }

            measureTime.Mark("After emulate itself");

            var material = GetComponent<Renderer>().material;

            material.SetInt("_TimeSize", timeSize);
            material.SetInt("_SpaceSize", spaceSize);

            var spaceSizePacked = spacetime[0].packedSize;
            var bufferSize = timeSize * spaceSizePacked;
            if (buffer is null || buffer.count != bufferSize) {
                buffer?.Dispose();
                buffer = new ComputeBuffer(bufferSize, sizeof(UInt32));
            }
            for (var t = 0; t < spacetime.Length; t++) {
                buffer.SetData(spacetime[t].packed, 0, t * spaceSizePacked, spaceSizePacked);
            }
            material.SetBuffer("_SpacetimePacked", buffer);
            measureTime.Mark("After shader");
        }
    }
    private ComputeBuffer buffer;
    private Dictionary<int, int> analyzeDict;
    public float renderAnalyzeNormalizationFactor;
    public int analyzeSkip = 2000;
    public int analyzeTake = 1000;

    public float f;
    [Button]
    public void Analyze() {
        using (var measureTime = new MeasureTime()) {
            spacetimeConvolution = new int[analyzeTake][];
            for (var t = analyzeSkip; t < analyzeSkip + analyzeTake; t++) {
                spacetimeConvolution[t - analyzeSkip] =
                    usePackedFillSpace
                        ? ConvolutePacked(spacetime[t], patternLength)
                        : Convolute(spacetime[t], patternLength);
            }

            measureTime.Mark("After Convolute");

            analyzeDict = new Dictionary<int, int>();
            analyzeDict[0] = 0;
            for (var t = analyzeSkip; t < analyzeSkip + analyzeTake; t++) {
                Analyze(spacetimeConvolution[t - analyzeSkip], patternLength, analyzeDict);
            }

            measureTime.Mark("After Analyze");

            // var s = string.Join(
            //     "\n", 
            //     analyzeDict
            //         .OrderByDescending(kv => kv.Value)
            //         .Select(kv => string.Join("", kv.Key) + " => " + kv.Value));
            // Debug.Log($"Patterns stats:\n{s}");

            measureTime.Mark("After Log");

            f = 0f;
            for (var t = analyzeSkip; t < analyzeSkip + analyzeTake; t++) {
                f += RenderAnalyze(spacetime[t], spacetimeConvolution[t - analyzeSkip], t, 10);
            }
            f = f / analyzeTake;

            measureTime.Mark("After RenderAnalyze");

            Debug.Log($"f {f}");

            // texture.Apply();
        }
    }
    public int[] ConvolutePacked(PackedSpace space, int size) {
        var packer = space.packer;

        var sizeCellShift = size * packer.cellSizeBits;
        Debug.Assert(sizeCellShift < 32, "Shift size exceeds 31");
        var sizeCellMask = (1 << sizeCellShift) - 1;
        var sizeLastCellShift = Packer.sizeOfPackBits - sizeCellShift;

        var lastCellShift = (size - 1) * packer.cellSizeBits;

        var convolution = new int[space.spaceSize];
        int prevPack = 0;
        var x = 0;
        for (var px = 0; px < space.packedSize; px++) {
            var pack = space.packed[px];
            for (var sx = 0; sx < packer.cellsPerPack; sx++) {
                if (x >= convolution.Length) {
                    return convolution;
                }

                var shift1 = (sx + 1) * packer.cellSizeBits;
                var shift2 = Packer.sizeOfPackBits - shift1;
                var mask2 = (1 << shift2) - 1;

                var key = (pack << shift2) | ((prevPack >> shift1) & mask2);

                convolution[x] = (key >> sizeLastCellShift) & sizeCellMask;

                x++;
            }
            prevPack = pack;
        }
        return convolution;
    }
    public int[] Convolute(Rule.ISpace<int> space, int size) {
        int keyDivider = 1;
        checked {
            for (var i = 0; i < size; i++) {
                keyDivider *= (int)stateCount;
            }
        }
        var convolution = new int[space.spaceSize];
        int key = 0;
        for (var x = 0; x < space.spaceSize; x++) {
            key = (key * (int)stateCount + (int)space[x]) % keyDivider;
            convolution[x] = key;
        }
        return convolution;
    }
    public void Analyze(
        int[] convolution,
        int size,
        Dictionary<int, int> dict
    ) {
        var nr = Rule.spaceNeighbourhoodRadius;
        for (var x = nr + size - 1; x < convolution.Length - nr; x++) {
            var key = convolution[x];
            dict.TryGetValue(key, out var i);
            dict[key] = i + 1;
        }
    }

    public float RenderAnalyze(Rule.ISpace<int> space, int[] convolution, int t, int size) {
        var counter = 0;
        var nr = Rule.spaceNeighbourhoodRadius;
        for (var x = nr + size + size; x < space.spaceSize - size - size - nr; x++) {
            var key = convolution[x + size - 1];

            var flag = true;
            for (var kx = 0; kx < size; kx++) {
                var k = convolution[x + kx];
                for (var i = kx + 1; i < size; i++) {
                    if (convolution[x + i] == k) {
                        flag = false;
                        break;
                    }
                }
            }
            // var c = texture.GetPixel(t, x);
            if (!flag) {
                // c.b = 0;
            } else {
                counter++;
                // var sum = 0;
                // for (var i = 0; i < length; i++) {
                //     sum += analyzeDict[pss[x + i]];
                // }
                // c.b = 0.5f + sum * renderAnalyzeNormalizationFactor;
            }
            // texture.SetPixel(t, x, c);
        }
        var normFactor = space.spaceSize - size - nr - nr;
        return counter * 1f / normFactor;
    }
}
