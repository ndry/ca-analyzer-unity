using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BigInteger = System.Numerics.BigInteger;
using System.Runtime.InteropServices;
using EasyButtons;
using UnityEngine;

public class EmulateOnShader : MonoBehaviour {

    public int stateCount = 3;
    public int spaceSize = 300;
    public int timeSize = 100;
    public int randSeed = 4242;
    public int parallelRules = 1;
    public bool inPlace;
    public int patternLength = 8;
    public RuleSpaceDesc ruleSpaceDesc = RuleSpaceDesc.Symmetrical;
    public ComputeShader shaderRef;
    public EmulateComputeShader shader;

    [Button]
    public void RandomizeRules() {
        var ruleSpace = ruleSpaceDesc.Create(stateCount);
        var packer = new Packer(stateCount);

        rules = Enumerable.Range(0, parallelRules)
            .Select(_ => new Rule(packer, ruleSpace.GenerateRandomFullTable(), EmulateComputeShader.tablePackN))
            .ToArray();
        rules[0] = new Rule(packer, new TotalisticRuleSpace { stateCount = stateCount }.GetFullTable(1815), EmulateComputeShader.tablePackN);
    }
    void OnValidate() {
        Test();
    }
    public Rule[] rules;
    [Button]
    public void UpdateRules() {
        var packer = new Packer(stateCount);
        rules = Enumerable.Range(0, parallelRules)
            .Select(i => transform.GetChild(i % transform.childCount))
            .Select(c => c.GetComponent<SpacetimeDisplay>())
            .Select(d => new Rule(packer, BigInteger.Parse(d.code), EmulateComputeShader.tablePackN))
            .ToArray();

    }
    [Button]
    public void Test() {
        if (shader is null || shader.shader != shaderRef) {
            shader?.DisposeBuffers();
            shader = new EmulateComputeShader(shaderRef);
        }
        using (var measureTime = new MeasureTime()) {
            // if (rules == null) {
            // UpdateRules();
            // measureTime.Mark("After UpdateRules");
            // }

            shader.Emulate(stateCount, spaceSize, timeSize, randSeed, rules, inPlace);


            measureTime.Mark("After Dispatch");

            var fs = new float[parallelRules];

            var packer = rules[0].packer;
            var timeSize1 = inPlace ? 2 : timeSize;
            var packedSpaceSize = packer.GetPackedSize(spaceSize);
            var layerSize = packedSpaceSize * timeSize1;
            for (var i = 0; i < parallelRules; i++) {
                var space = new PackedSpace(packer, spaceSize);
                shader.spacetimePackedBuffer.GetData(
                    space.packed,
                    0,
                    layerSize * i + packedSpaceSize * (timeSize1 - 1),
                    packedSpaceSize);
                fs[i] = RuleSpacetimeAnalyzer.RenderAnalyze(packer, spaceSize, space.Convolute(), patternLength);
            }

            measureTime.Mark("After Analyze");

            SetDisplays(rules.Select(rule => rule.code.ToString()).ToArray(), fs);

            measureTime.Mark("After SetDisplays");
        }
    }

    public Vector3 midOffset = new Vector3(11, 0, 0);
    public Vector3 fOffset = new Vector3(0, 20, 0);
    [Button]
    public void SetDisplays(string[] codes, float[] fs) {
        for (var i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            var display = child.GetComponent<SpacetimeDisplay>();
            var ic = i % codes.Length;
            display.SetSpacetime(
                shader.spacetimePackedBuffer,
                spaceSize,
                inPlace ? 2 : timeSize,
                codes[ic],
                fs[ic],
                ic);
            child.localPosition =
                midOffset * i + fs[ic] * fOffset;
        }
    }

    [Button]
    public void DisposeBuffer() {
        shader?.DisposeBuffers();
    }

    void OnDestroy() {
        DisposeBuffer();
    }
}
