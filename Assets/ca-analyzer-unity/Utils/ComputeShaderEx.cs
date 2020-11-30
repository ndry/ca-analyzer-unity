using UnityEngine;

public static class ComputeShaderEx {
    public static void Set(this ComputeShader shader, string name, int value)
        => shader.SetInt(name, value);
    public static void EnsureBuffer(ref ComputeBuffer buffer, int count, int stride) {
        if (buffer != null && (buffer.count != count || buffer.stride != stride)) {
            buffer.Dispose();
            buffer = null;
        }
        if (buffer == null) {
            buffer = new ComputeBuffer(count, stride);
        }
        Debug.Log($"EnsureBuffer size {count}");
    }
    public static void EnsureConstantBuffer(ref ComputeBuffer buffer, int[] data) {
        EnsureBuffer(ref buffer, data.Length, sizeof(int));
        buffer.SetData(data);
    }
}
