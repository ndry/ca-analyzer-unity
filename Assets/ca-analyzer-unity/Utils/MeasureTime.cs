using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class MeasureTimeStub : IDisposable {
    public string name;
    public List<string> log = new List<string>();
    public double lastTotalMs = 0;
    public System.Diagnostics.Stopwatch watch;
    public MeasureTimeStub([CallerMemberName] string caller = null) {
        name = caller;
        watch = System.Diagnostics.Stopwatch.StartNew();
    }

    public void Mark(string markName = null) {
        watch.Stop();
        watch.Start();
    }

    public void Dispose() {
        watch.Stop();
    }
}

public class MeasureTime : IDisposable {
    public string name;
    public List<string> log = new List<string>();
    public double lastTotalMs = 0;
    public System.Diagnostics.Stopwatch watch;
    public DateTime startDate;
    public MeasureTime([CallerMemberName] string caller = null) {
        name = caller;
        startDate = DateTime.UtcNow;
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
        var dt = DateTime.UtcNow - startDate;
        var controlTotalMs = dt.TotalMilliseconds;
        if (log.Count > 0) {
            details = $" Details: \n{string.Join("\n", log)}";
        }
        Debug.Log($"{name} time (ms) {totalMs} (control {controlTotalMs}).{details}\n\n");
    }
}
