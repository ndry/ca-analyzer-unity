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
    [Button]
    public void TakeScreenshot() {
        var filename = $"rule_{rule?.ruleCode}_{DateTime.Now:yyyy_MM_ddThh_mm_ss}.png";
        Directory.CreateDirectory("Screenshots");
        ScreenCapture.CaptureScreenshot(Path.Combine("Screenshots", filename));
        Debug.Log($"Screenshot taken {filename}");
    }

    [Button]
    public void Emulate() {
        rule?.Emulate();
    }

    [Button]
    public void Randomate() {
        rule?.RandomizeRule();
        rule?.Emulate();
    }
}
