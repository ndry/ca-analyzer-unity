using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;

[RequireComponent(typeof(RuleSpacetime))]
public class RuleSpacetimeAnalyzer : MonoBehaviour {
    public float renderAnalyzeNormalizationFactor;
    public int analyzeSkip = 2000;
    public int analyzeTake = 1000;
    private int[][] spacetimeConvolution;
    public int patternLengthStart = 10;
    public int patternLengthCount = 1;
    public BarChart barChart;
    public BarChart barChart1;

    public float f;
    [Button]
    public void Analyze() {
        var ruleSpacetime = GetComponent<RuleSpacetime>();
        var spacetime = ruleSpacetime.spacetime;
        var stateCount = ruleSpacetime.stateCount;

        spacetimeConvolution = new int[analyzeTake][];
        using (var measureTime = new MeasureTimeStub()) {
            for (var t = analyzeSkip; t < analyzeSkip + analyzeTake; t++) {
                spacetimeConvolution[t - analyzeSkip]
                    = spacetime[t].Convolute();
            }
            measureTime.Mark("After Convolute");
            for (
                var size = patternLengthStart;
                size < patternLengthStart + patternLengthCount;
                size++
            ) {
                f = 0f;
                for (var t = analyzeSkip; t < analyzeSkip + analyzeTake; t++) {
                    f += RenderAnalyze(spacetime[t], spacetimeConvolution[t - analyzeSkip], size);
                }
                f = f / analyzeTake;

                measureTime.Mark("After RenderAnalyze");

                Debug.Log($"f {f}");
            }
        }
    }
    public static float RenderAnalyze(PackedSpace space, int[] convolution, int size)
        => RenderAnalyze(space.packer, space.spaceSize, convolution, size);
    public static float RenderAnalyze(Packer packer, int spaceSize, int[] convolution, int size) {
        var dict = new Dictionary<int, (int, int)>();
        var nr = Rule.spaceNeighbourhoodRadius;
        Debug.Assert(size <= packer.cellsPerPack);
        var sizeMask =
            (size == packer.cellsPerPack)
                ? -1
                : ~(-1 << (size * packer.cellSizeBits));
        for (var x = nr + size; x < spaceSize - nr; x++) {
            var key = convolution[x] & sizeMask;
            dict.TryGetValue(key, out var val);
            val.Item2++;

            var flag = true;
            for (var kx = 1; kx < size; kx++) {
                var k = convolution[x - kx] & sizeMask;
                if (key == k) {
                    flag = false;
                    break;
                }
            }
            if (flag) {
                val.Item1++;
            }
            dict[key] = val;
        }
        // var s = string.Join(
        //     "\n",
        //     dict
        //         .OrderByDescending(kv => kv.Value.Item1)
        //         .ThenByDescending(kv => kv.Value.Item2)
        //         .Select(kv => string.Join("", RuleConverterLib.GetDigitsFromNumber(kv.Key, 4)) + " => " + kv.Value));
        // Debug.Log($"Patterns stats:\n{s}");
        // barChart.data = dict
        //     .Select(kv => (float)kv.Value.Item1)
        //     .OrderByDescending(x => x)
        //     .ToList();
        // barChart.Refresh();
        // barChart1.data = dict
        //     .Select(kv => (float)kv.Value.Item2)
        //     .OrderByDescending(x => x)
        //     .ToList();
        // barChart1.Refresh();
        var data = dict
            .Select(kv => (float)kv.Value.Item2)
            .OrderByDescending(x => x)
            .ToList();
        var f = (data.Count > 0 ? Mathf.Log(data[1], data[0]) : 1);
        return f;
    }
}
