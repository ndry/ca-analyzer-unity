using System;
using System.Collections.Generic;
using Random = System.Random;

public class Emulator {
    public enum StartFill {
        Zeros, Random
    }
    public enum BorderFill {
        Cycle, Zeros, Random
    }

    private Rule rule;
    private int timeSize;
    private int spaceSize;
    private StartFill startFill;
    private BorderFill borderFill;
    private Random random;
    Action<PackedSpace> fillStart;
    Action<PackedSpace> fillBorder;

    public Emulator(
        Rule rule,
        int timeSize,
        int spaceSize,
        StartFill startFill,
        BorderFill borderFill,
        Random random
    ) {
        this.rule = rule;
        this.timeSize = timeSize;
        this.spaceSize = spaceSize;
        this.startFill = startFill;
        this.borderFill = borderFill;
        this.random = random;

        switch (startFill) {
            case StartFill.Zeros:
                fillStart = FillStartZeros;
                break;
            case StartFill.Random:
                fillStart = FillStartRandom;
                break;
            default:
                throw new NotSupportedException();
        }

        switch (borderFill) {
            case BorderFill.Cycle:
                fillBorder = FillBorderCycle;
                break;
            case BorderFill.Zeros:
                fillBorder = FillBorderZeros;
                break;
            case BorderFill.Random:
                fillBorder = FillBorderRandom;
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public PackedSpace EmulateInPlace() {
        var packer = rule.packer;

        var prevPrevSpace = new PackedSpace(packer, spaceSize);
        fillStart(prevPrevSpace);

        var prevSpace = new PackedSpace(packer, spaceSize);
        fillStart(prevSpace);

        var space = new PackedSpace(packer, spaceSize);
        for (var t = 2; t < timeSize; t++) {
            rule.FillSpace(space, prevSpace, prevPrevSpace);
            fillBorder(space);

            var tmpSpace = prevPrevSpace;
            prevPrevSpace = prevSpace;
            prevSpace = space;
            space = prevPrevSpace;
        }

        return space;
    }

    public IEnumerable<PackedSpace> Emulate() {
        var packer = rule.packer;

        var prevPrevSpace = new PackedSpace(packer, spaceSize);
        fillStart(prevPrevSpace);
        yield return prevPrevSpace;

        var prevSpace = new PackedSpace(packer, spaceSize);
        fillStart(prevSpace);
        yield return prevSpace;

        for (var t = 2; t < timeSize; t++) {
            var space = new PackedSpace(packer, spaceSize);
            rule.FillSpace(space, prevSpace, prevPrevSpace);
            fillBorder(space);
            yield return space;

            prevPrevSpace = prevSpace;
            prevSpace = space;
        }
    }

    public void Emulate(PackedSpace[] spacetime) {
        fillStart(spacetime[0]);
        fillStart(spacetime[1]);
        for (var t = 2; t < timeSize; t++) {
            var space = rule.FillSpace(spacetime, t);
            fillBorder(space);
        }
    }

    private void FillStartZeros(PackedSpace space) {
        for (var x = 0; x < spaceSize; x++) {
            space[x] = 0;
        }
    }

    private void FillStartRandom(PackedSpace space) {
        var stateCount = rule.stateCount;
        for (var x = 0; x < spaceSize; x++) {
            space[x] = random.Next(stateCount);
        }
    }

    public void FillBorderCycle(PackedSpace space) {
        var snr = Rule.spaceNeighbourhoodRadius;
        for (var x = 0; x < snr; x++) {
            space[x] = space[spaceSize - snr - snr + x];
            space[spaceSize - snr + x] = space[snr + x];
        }
    }
    public void FillBorderZeros(PackedSpace space) {
        var snr = Rule.spaceNeighbourhoodRadius;
        for (var x = 0; x < snr; x++) {
            space[x] = 0;
            space[spaceSize - snr + x] = 0;
        }
    }
    public void FillBorderRandom(PackedSpace space) {
        var snr = Rule.spaceNeighbourhoodRadius;
        var stateCount = rule.stateCount;
        for (var x = 0; x < snr; x++) {
            space[x] = random.Next(stateCount);
            space[spaceSize - snr + x] = random.Next(stateCount);
        }
    }
}