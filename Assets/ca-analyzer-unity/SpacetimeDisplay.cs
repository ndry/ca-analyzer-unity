using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacetimeDisplay : MonoBehaviour {
    public bool keepAspectRatio = true;
    public string code;
    public float f;
    public void SetSpacetime(
        ComputeBuffer spacetimePackedBuffer,
        int spaceSize,
        int timeSize,
        string code,
        float f,
        int layer = 0
    ) {
        this.code = code;
        this.f = f;
        if (keepAspectRatio) {
            transform.localScale = new Vector3(
                transform.localScale.z * timeSize / spaceSize,
                transform.localScale.y,
                transform.localScale.z);
        }
        var material = GetComponent<Renderer>().material;

        material.SetInt("_TimeSize", timeSize);
        material.SetInt("_SpaceSize", spaceSize);
        material.SetInt("_Layer", layer);
        material.SetBuffer("_SpacetimePacked", spacetimePackedBuffer);
    }
}
