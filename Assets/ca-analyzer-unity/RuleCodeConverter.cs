using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using EasyButtons;
using UnityEngine;

[RequireComponent(typeof(RuleSpacetime))]
public class RuleCodeConverter : MonoBehaviour {
    public RuleSpaceDesc ruleSpaceDesc;
    public string code;
    public bool setToRuleSpace = true;
    public string convertedCode;
    public RuleSpacetime ruleSpacetime => GetComponent<RuleSpacetime>();
    public int stateCount => ruleSpacetime.stateCount;
    void OnValidate() {
        Convert();
    }
    [Button]
    public void Randomize() {
        var ruleSpace = ruleSpaceDesc.Create(stateCount);
        code = ruleSpace.GenerateRandomCodeString();
        convertedCode = ruleSpace.GetFullCode(code);
        if (setToRuleSpace) {
            SetToRuleSpace();
        }
    }
    [Button]
    public void Convert() {
        var ruleSpace = ruleSpaceDesc.Create(stateCount);
        convertedCode = ruleSpace.GetFullCode(code);
        if (setToRuleSpace) {
            SetToRuleSpace();
        }
    }
    [Button]
    public void SetToRuleSpace() {
        ruleSpacetime.stateCount = stateCount;
        ruleSpacetime.ruleCode = convertedCode;
    }
    [Button(ButtonSpacing.Before)]
    public void Randomate() {
        var ruleSpace = ruleSpaceDesc.Create(stateCount);
        code = ruleSpace.GenerateRandomCodeString();
        convertedCode = ruleSpace.GetFullCode(code);
        SetToRuleSpace();
        ruleSpacetime.Emulate();
    }

}
