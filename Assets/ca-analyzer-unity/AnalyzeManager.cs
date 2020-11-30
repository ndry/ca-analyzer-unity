using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;
using EasyButtons;
using UnityEngine;

public class AnalyzeManager : MonoBehaviour {
    public Vector3 midOffset = new Vector3(120, 0, 0);
    public Vector3 fOffset = new Vector3(0, 200, 0);
    public bool randomizeRules = false;

    public int stateCount = 3;
    public RuleSpaceDesc ruleSpaceDesc;
    public int timeSize = 8000;
    public int spaceSize = 4000;
    public Emulator.StartFill startFill;
    public Emulator.BorderFill borderFill;
    public int seed = 4242;
    [Button]
    public void Analyze2() {
        using (var measureTime = new MeasureTime()) {
            var count = transform.childCount;
            var ruleSpace = ruleSpaceDesc.Create(stateCount);
            var packer = new Packer(stateCount);
            var p = Enumerable.Range(0, count)
                .Select(_ => ruleSpace.GenerateRandomTable())
                .ToArray()
                .AsParallel()
                .Select(table => new Rule(packer, ruleSpace.GetFullTable(table), 2))
                .Select(rule => new Emulator(rule, timeSize, spaceSize, startFill, borderFill, new Random(seed)))
                .Select(emulator => emulator.EmulateInPlace())
                .ToArray();
        }
    }
    [Button]
    public void Analyze3() {
        using (var measureTime = new MeasureTime()) {
            var count = transform.childCount;
            var ruleSpace = ruleSpaceDesc.Create(stateCount);
            var packer = new Packer(stateCount);
            var p = Enumerable.Range(0, count)
                .Select(_ => new Rule(packer, ruleSpace.GenerateRandomFullTable(), 2))
                .Select(rule => new Emulator(rule, timeSize, spaceSize, startFill, borderFill, new Random(seed)))
                .Select(emulator => emulator.EmulateInPlace())
                .ToArray();
        }
    }
    [Button]
    public void Analyze() {
        using (var watch = new MeasureTime()) {
            var targets = GetComponentsInChildren<RuleSpacetime>();
            watch.Mark("After get targets");
            foreach (var target in targets) {
                if (randomizeRules) {
                    var converter = target.GetComponent<RuleCodeConverter>();
                    converter.Randomize();
                }
            }
            watch.Mark("After Randomize");
            foreach (var target in targets) {
                target.Emulate();
            }
            watch.Mark("After Emulate");
            foreach (var target in targets) {
                var analyzer = target.GetComponent<RuleSpacetimeAnalyzer>();
                analyzer.Analyze();
            }
            watch.Mark("After Analyze");
            targets
                .OrderBy(t => t.GetComponent<RuleSpacetimeAnalyzer>().f)
                .Do((target, i) => {
                    target.transform.parent.localPosition =
                        midOffset * i
                        + fOffset * target.GetComponent<RuleSpacetimeAnalyzer>().f;
                })
                .ToArray();
            watch.Mark("After sort and position");
        }
    }
}
