using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPath : MonoBehaviour {
    public RuleSpacetime rule;
    public float zoomVelocity = 0f;
    public float zoomVelocityPeriod = 1f;
    public Vector3 velocity;
    // Start is called before the first frame update
    void Start() {
        rule.Emulate();
    }

    // Update is called once per frame
    void Update() {
        transform.Translate(Time.fixedDeltaTime * velocity);
        var camera = GetComponent<Camera>();
        camera.orthographicSize *= Mathf.Exp(
            zoomVelocity * Mathf.Sin(Time.timeSinceLevelLoad * zoomVelocityPeriod));
    }
}
