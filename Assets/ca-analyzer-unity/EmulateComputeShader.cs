using UnityEngine;

public class EmulateComputeShader {
    public static bool usePackedPacked => true;
    public static int tablePackN => 2;


    public ComputeBuffer spacetimePackedBuffer;
    public ComputeBuffer ruleTablePackedNCellBuffer;
    public ComputeBuffer flagBuffer;

    public ComputeShader shader;
    public ShaderKernel fillStartRandomKernel
        => FindKernel("CSFillStartRandom");

    public EmulateComputeShader(ComputeShader shader) {
        this.shader = shader;
    }

    public void Emulate(
        int stateCount,
        int spaceSize,
        int timeSize,
        int randSeed,
        Rule[] rules,
        bool inPlace
    ) {
        Debug.Assert(rules[0].tablePackN == tablePackN);

        var packer = rules[0].packer;
        var spaceSizePacked = packer.GetPackedSize(spaceSize);

        var timestepsPerCall = (timeSize - Rule.timeNeighbourhoodRadius);

        if (usePackedPacked) {
            var packedRule = new PackedSpace(
                new Packer(4 * 4),
                rules[0].tablePackedNCell.Length);
            ComputeShaderEx.EnsureBuffer(
                ref ruleTablePackedNCellBuffer,
                rules.Length * packedRule.packed.Length,
                sizeof(int));

            for (var i = 0; i < rules.Length; i++) {
                var rule = rules[i];
                for (var j = 0; j < rule.tablePackedNCell.Length; j++) {
                    packedRule[j] = rule.tablePackedNCell[j];
                }
                ruleTablePackedNCellBuffer.SetData(
                    packedRule.packed,
                    0,
                    i * packedRule.packed.Length,
                    packedRule.packed.Length);
            }
        } else {
            ComputeShaderEx.EnsureBuffer(
                ref ruleTablePackedNCellBuffer,
                rules.Length * rules[0].tablePackedNCell.Length,
                sizeof(int));

            for (var i = 0; i < rules.Length; i++) {
                var rule = rules[i];
                ruleTablePackedNCellBuffer.SetData(
                    rule.tablePackedNCell,
                    0,
                    i * rule.tablePackedNCell.Length,
                    rule.tablePackedNCell.Length);
            }
        }

        var spacetimePackedBufferSize =
            rules.Length * spaceSizePacked * (inPlace ? 2 : timeSize);
        ComputeShaderEx.EnsureBuffer(
            ref spacetimePackedBuffer, spacetimePackedBufferSize, sizeof(int));
        ComputeShaderEx.EnsureBuffer(
            ref flagBuffer, 1, sizeof(int));

        shader.Set("_RandSeed", randSeed);
        shader.Set("_SpaceSize", spaceSize);
        shader.Set("_TimeSize", inPlace ? 2 : timeSize);
        shader.Set("_TimeTo", timeSize);

        var fillStartRandomKernel = this.fillStartRandomKernel;
        fillStartRandomKernel.SetBuffer(
            "_SpacetimePacked", spacetimePackedBuffer);
        fillStartRandomKernel.Dispatch(spaceSizePacked, rules.Length);

        var emulateKernelName = "CSEmulate";
        var emulateKernel =
            new ShaderKernel(shader, emulateKernelName);
        emulateKernel.Set(
            "_SpacetimePacked", spacetimePackedBuffer);
        emulateKernel.Set(
            "_Flag", flagBuffer);
        emulateKernel.Set(
            "_RuleTablePackedNCell", ruleTablePackedNCellBuffer);
        var tnr = Rule.timeNeighbourhoodRadius;

        for (var t = tnr; t < timeSize; t += timestepsPerCall) {
            shader.SetInt("_TimeStep", t);
            emulateKernel.Dispatch(
                spaceSizePacked,
                rules.Length);
        }

        var data = new int[1];
        flagBuffer.GetData(data);
    }

    public void DisposeBuffers() {
        if (spacetimePackedBuffer != null) {
            spacetimePackedBuffer.Dispose();
            spacetimePackedBuffer = null;
        }
        if (ruleTablePackedNCellBuffer != null) {
            ruleTablePackedNCellBuffer.Dispose();
            ruleTablePackedNCellBuffer = null;
        }
        if (flagBuffer != null) {
            flagBuffer.Dispose();
            flagBuffer = null;
        }
    }

    public ShaderKernel FindKernel(string name)
        => shader.FindKernelWrap(name);
}
