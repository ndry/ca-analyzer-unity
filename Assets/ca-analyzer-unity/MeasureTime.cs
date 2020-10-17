using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class MeasureTime : IDisposable {
    public string name;
    public List<string> log = new List<string>();
    public double lastTotalMs = 0;
    public System.Diagnostics.Stopwatch watch;
    public MeasureTime([CallerMemberName] string caller = null) {
        name = caller;
        watch = System.Diagnostics.Stopwatch.StartNew();
    }

    public void Mark(string markName = null) {
        watch.Stop();
        markName = markName ?? $"Mark {log.Count + 1}";
        var totalMs = watch.Elapsed.TotalMilliseconds;
        log.Add($"{markName}    {totalMs} (+{totalMs - lastTotalMs})");
        lastTotalMs = totalMs;
        watch.Start();
    }

    public void Dispose() {
        watch.Stop();
        var totalMs = watch.Elapsed.TotalMilliseconds;
        var details = "";
        if (log.Count > 0) {
            details = $" Details: \n{string.Join("\n", log)}";
        }
        Debug.Log($"{name} time (ms) {totalMs}.{details}\n\n");
    }
}
