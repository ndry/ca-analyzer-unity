using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;

public class AnalyzeManager : MonoBehaviour
{
    public Vector3 midOffset = new Vector3(120, 0, 0);
    public Vector3 fOffset = new Vector3(0, 200, 0);
    public bool randomizeRules = false;
    public bool analyze = false;
    [Button]
    public void Analyze() {
        var targets = GetComponentsInChildren<RuleSpacetime>();
        foreach (var target in targets) {
            if (analyze) {
                if (randomizeRules) {
                    target.RandomizeRule();
                }
                // var watch = System.Diagnostics.Stopwatch.StartNew();
                target.Emulate();
                // Debug.Log($"After Emulate {watch.ElapsedMilliseconds}");
                target.Analyze();
                // watch.Stop();
                // Debug.Log($"After Analyze {watch.ElapsedMilliseconds}");
            }            
        }
        targets
            .OrderBy(t => t.f)
            .Do((target, i) => {
                target.transform.localPosition = 
                    midOffset * i 
                    + fOffset * target.f;
            })
            .ToArray();
    }
} 
