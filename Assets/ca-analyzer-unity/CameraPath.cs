using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPath : MonoBehaviour {
    public RuleSpacetime rule;
    public float zoomVelocity = 1;
    public Vector3 velocity;

    public float switchZoomAt = 100;
    // Start is called before the first frame update
    void Start() {
        rule.Emulate();
    }

    // Update is called once per frame
    void Update() {
        transform.Translate(Time.deltaTime * velocity);
        var camera = GetComponent<Camera>();
        camera.orthographicSize *= Mathf.Pow(zoomVelocity, Time.deltaTime);
        if (switchZoomAt >= 0 && zoomVelocity > 0 && camera.orthographicSize > switchZoomAt) {
            zoomVelocity = 1 / zoomVelocity;
        }
    }
}
