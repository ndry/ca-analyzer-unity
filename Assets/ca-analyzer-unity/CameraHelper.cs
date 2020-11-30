using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EasyButtons;
using UnityEngine;

[ExecuteAlways]
public class CameraHelper : MonoBehaviour {
    public RuleSpacetime rule;
    void OnValidate() {
        UpdateRule();
    }
    void Update() {
        UpdateRule();
    }

    public void UpdateRule() {
        if (Physics.Raycast(transform.position, transform.forward, out var hit)) {
            rule = hit.transform.GetComponentInParent<RuleSpacetime>();
        } else {
            rule = null;
        }
    }
    Transform Hit() {
        Physics.Raycast(transform.position, transform.forward, out var hit);
        return hit.transform;
    }
    [Button]
    public void TakeScreenshot() {
        UpdateRule();
        var hit = Hit();
        Debug.Log(hit);
        var rule = hit?.GetComponent<RuleSpacetime>();
        var sd = hit?.GetComponent<SpacetimeDisplay>();
        var filename = $"rule_{rule?.ruleCode ?? sd?.code}_{DateTime.Now:yyyy_MM_ddThh_mm_ss}.png";
        Directory.CreateDirectory("Screenshots");
        ScreenCapture.CaptureScreenshot(Path.Combine("Screenshots", filename));
        Debug.Log($"Screenshot taken {filename}");
    }

    [Button]
    public void Emulate() {
        UpdateRule();
        if (!rule) {
            return;
        }
        rule.Emulate();
    }

    [Button]
    public void Randomatyze() {
        UpdateRule();
        if (!rule) {
            return;
        }
        var ruleCodeConverter = rule.GetComponent<RuleCodeConverter>();
        var analyzer = rule.GetComponent<RuleSpacetimeAnalyzer>();
        ruleCodeConverter.Randomate();
        analyzer.Analyze();
    }
}
