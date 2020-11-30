using System.Collections;
using BigInteger = System.Numerics.BigInteger;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using System;
using Random = System.Random;

public class RuleSpacetime : MonoBehaviour {
    [Serializable]
    public class EmulationParams {
        public int timeSize = 800;
        public int spaceSize = 400;
        public Emulator.StartFill startFill;
        public Emulator.BorderFill borderFill;
        public int seed = 4242;
        public bool useSeed = true;
    }
    public int stateCount = 3;
    public string ruleCode;
    public EmulationParams emulationParams;
    public bool keepAspectRatio = true;
    public PackedSpace[] spacetime;
    public int timeSize => emulationParams.timeSize;
    public int spaceSize => emulationParams.spaceSize;
    public Emulator.StartFill startFill => emulationParams.startFill;
    public Emulator.BorderFill borderFill => emulationParams.borderFill;
    public int seed => emulationParams.seed;
    public bool useSeed => emulationParams.useSeed;

    void OnValidate() {
        if (keepAspectRatio) {
            transform.localScale = new Vector3(
                transform.localScale.z * timeSize / spaceSize,
                transform.localScale.y,
                transform.localScale.z);
        }
    }
    public int tablePackN = 2;

    [Button]
    public void Emulate() {
        using (var measureTime = new MeasureTime()) {

            var random = useSeed ? new Random(seed) : new Random();

            var packer = new Packer(stateCount);
            var rule = new Rule(packer, BigInteger.Parse(ruleCode), tablePackN);

            measureTime.Mark("After rule inst");
            if (spacetime == null || spacetime.Length != timeSize || spacetime[0].spaceSize != spaceSize) {
                spacetime = new PackedSpace[timeSize];
                for (var t = 0; t < spacetime.Length; t++) {
                    spacetime[t] = new PackedSpace(packer, spaceSize);
                }
            }
            var emulator = new Emulator(
                rule, timeSize, spaceSize, startFill, borderFill, random);

            measureTime.Mark("After raw spacetime creation");
            emulator.Emulate(spacetime);

            measureTime.Mark("After emulate itself");

            var material = GetComponent<Renderer>().material;

            material.SetInt("_TimeSize", timeSize);
            material.SetInt("_SpaceSize", spaceSize);

            var spaceSizePacked = spacetime[0].packedSize;
            var bufferSize = timeSize * spaceSizePacked;
            if (buffer is null || buffer.count != bufferSize) {
                buffer?.Dispose();
                buffer = new ComputeBuffer(bufferSize, sizeof(UInt32));
            }
            for (var t = 0; t < spacetime.Length; t++) {
                buffer.SetData(spacetime[t].packed, 0, t * spaceSizePacked, spaceSizePacked);
            }
            material.SetBuffer("_SpacetimePacked", buffer);
            measureTime.Mark("After shader");
        }
    }
    private ComputeBuffer buffer;
}
