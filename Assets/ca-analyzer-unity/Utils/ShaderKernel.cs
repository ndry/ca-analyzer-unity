using UnityEngine;

public class ShaderKernel {
    public readonly ComputeShader shader;
    public readonly string kernelName;
    public readonly int kernelIndex;
    public readonly Vector3Int threadGroupSizes;
    public ShaderKernel(ComputeShader shader, string kernelName) {
        this.shader = shader;
        this.kernelName = kernelName;
        kernelIndex = shader.FindKernel(kernelName);
        shader.GetKernelThreadGroupSizes(
            kernelIndex, out var x, out var y, out var z);
        threadGroupSizes = new Vector3Int((int)x, (int)y, (int)z);
    }
    public void SetBuffer(string nameID, ComputeBuffer buffer)
        => shader.SetBuffer(kernelIndex, nameID, buffer);
    public void Set(string nameID, ComputeBuffer buffer)
        => SetBuffer(nameID, buffer);
    public void Dispatch(int x = 1, int y = 1, int z = 1)
        => shader.Dispatch(kernelIndex,
            1 + (x - 1) / threadGroupSizes.x,
            1 + (y - 1) / threadGroupSizes.y,
            1 + (z - 1) / threadGroupSizes.z);
}

public static class ShaderKernelEx {
    public static ShaderKernel FindKernelWrap(this ComputeShader shader, string name)
        => new ShaderKernel(shader, name);
}