using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BarChart : MonoBehaviour {
    public Transform bars;
    public Vector3 barOffset = new Vector3(20, 0, 0);
    public List<float> data;
    public int take = 100;
    void OnValidate() {
        Refresh();
    }
    public void Refresh() {
        var prototype = bars.GetChild(0);
        var d = data.Take(take).ToList();
        for (var i = 0; i < d.Count; i++) {
            if (i >= bars.childCount) {
                GameObject.Instantiate(prototype, bars);
            }
            var bar = bars.GetChild(i);
            bar.gameObject.SetActive(true);
            bar.localPosition = i * barOffset;
            bar.localScale = new Vector3(1, 1, d[i]);
        }
        for (var i = d.Count; i < bars.childCount; i++) {
            var bar = bars.GetChild(i);
            bar.gameObject.SetActive(false);
        }
    }
}
