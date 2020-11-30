#ifndef CA_RULE_INCLUDED
#define CA_RULE_INCLUDED

#include "ShaderLib/Utils.hlsl"
#include "ShaderLib/Packer.hlsl"

// #define CA_RULE_STRAIGHTFORWARD
#ifdef CA_RULE_STRAIGHTFORWARD

Buffer<uint> _RuleTable;

uint fillPack_StraightForward(uint ptm2c, uint ptm1n2, uint ptm1c, uint ptm1n1) {
    uint result = 0;
    for (uint xs = 0; xs < CELLS_PER_PACK; xs++) {
        uint pc = getFromPack(ptm2c, xs);
        uint n1 = 
            (xs > 0)
                ? getFromPack(ptm1c, xs - 1)
                : getFromPack(ptm1n1, CELLS_PER_PACK - 1);
        uint c = getFromPack(ptm1c, xs);
        uint n2 = 
            (xs < CELLS_PER_PACK - 1)
                ? getFromPack(ptm1c, xs + 1)
                : getFromPack(ptm1n2, 0);

        uint combinedState
            = ((pc * STATE_COUNT + n2) * STATE_COUNT + c) * STATE_COUNT + n1;

        result = setIntoPack(result, xs, _RuleTable[combinedState]);
    }
    return result;
}

uint fillPack_StraightForward1(uint ptm2c, uint ptm1n2, uint ptm1c, uint ptm1n1) {
    static const uint4 off4 = uint4(0, 1, 2, 3);

    uint _ptm1n1 = appendToPack(ptm1n1, ptm1c, CELLS_PER_PACK - 1);
    uint _ptm1n2 = appendToPack(ptm1c, ptm1n2, 1);

    uint result = 0;
    for (uint xs = 0; xs < CELLS_PER_PACK / 4; xs++) {
        uint4 xs4 = xs * 4 + off4;
        uint4 pc = getFromPack(ptm2c, xs4);
        uint4 n1 = getFromPack(_ptm1n1, xs4);
        uint4 c = getFromPack(ptm1c, xs4);
        uint4 n2 = getFromPack(_ptm1n2, xs4);
        
        uint combinedState
            = ((pc * STATE_COUNT + n2) * STATE_COUNT + c) * STATE_COUNT + n1;

        result = setIntoPack(result, xs4.x, _RuleTable[combinedState.x]);
        result = setIntoPack(result, xs4.y, _RuleTable[combinedState.y]);
        result = setIntoPack(result, xs4.z, _RuleTable[combinedState.z]);
        result = setIntoPack(result, xs4.w, _RuleTable[combinedState.w]);
    }
    return result;
}

#endif // CA_RULE_STRAIGHTFORWARD

#define CA_RULE_PACKEDNCELL
#ifdef CA_RULE_PACKEDNCELL

#define TABLE_PACK_N 2
#define TABLE_PACKED_PACKED
#define TABLE_PACKED_PACKED_CELL_SIZE (TABLE_PACK_N * CELL_SIZE_BITS)
#define TABLE_PACKED_PACKED_CELLS_PER_PACK (SIZE_OF_PACK_BITS / TABLE_PACKED_PACKED_CELL_SIZE)
Buffer<uint> _RuleTablePackedNCell;
uint getStatePackedNCell(uint layer, uint combinedState) {
    static const uint n = TABLE_PACK_N;
    static const uint indexSize = n + n + 2;
    static const uint layerSize = 1 << cellSize(indexSize);
    
#ifdef TABLE_PACKED_PACKED
    static const uint packedLayerSzie = (layerSize - 1) / TABLE_PACKED_PACKED_CELLS_PER_PACK + 1;
    uint tableStart = layer * packedLayerSzie;
    uint i = combinedState;
    uint pi = i / TABLE_PACKED_PACKED_CELLS_PER_PACK;
    uint si = i % TABLE_PACKED_PACKED_CELLS_PER_PACK;
    uint iCellShift = si * TABLE_PACKED_PACKED_CELL_SIZE;
    uint _1CellMask = ~(-1 << TABLE_PACKED_PACKED_CELL_SIZE);
    uint iPack = _RuleTablePackedNCell[tableStart + pi];
    uint state =  (iPack >> iCellShift) & _1CellMask;
    return state;
#else 
    uint tableStart = layer * layerSize;
    return _RuleTablePackedNCell[tableStart + combinedState];
#endif
}
uint getStatePackedNCell(uint layer, uint _pcn, uint _n1cnn2) {
    static const uint n = TABLE_PACK_N;
    static const uint indexSize = n + n + 2;
    uint combinedState = 
        ((_pcn << cellSize(n + 2)) | (_n1cnn2 & cellMask(n + 2)))
        & cellMask(indexSize);
    return getStatePackedNCell(layer, combinedState);
}
uint4 getStatePackedNCell(uint layer, uint4 _pcn, uint4 _n1cnn2) {
    static const uint n = TABLE_PACK_N;
    static const uint indexSize = n + n + 2;
    uint4 combinedState = 
        ((_pcn << cellSize(n + 2)) | (_n1cnn2 & cellMask(n + 2)))
        & cellMask(indexSize);
    return uint4(
        getStatePackedNCell(layer, combinedState[0]),
        getStatePackedNCell(layer, combinedState[1]),
        getStatePackedNCell(layer, combinedState[2]),
        getStatePackedNCell(layer, combinedState[3])
    );
}
uint getStatePackedNCell4(uint layer, uint _pcn, uint _n1cnn2) {
    static const uint n = TABLE_PACK_N;
    static const uint4 offsets = cellSize(uint4(0, 1, 2, 3) * n);
    uint4 state = 
        getStatePackedNCell(layer, _pcn >> offsets, _n1cnn2 >> offsets) << offsets;
    return state[0] | state[1] | state[2] | state[3];
}

uint fillPack(uint layer, uint ptm2c, uint ptm1n2, uint ptm1c, uint ptm1n1) {
    static const uint n = TABLE_PACK_N;

    uint pack = 0;

    uint ptm1cn1 = appendToPack(ptm1n1, ptm1c, CELLS_PER_PACK - 1);

    uint sx = 0;
    // // Vector optimization trial
    // [unroll]
    // for (; sx < (CELLS_PER_PACK - n * 4 - 2); sx += n * 4) {
    //     uint state = getStatePackedNCell4(
    //         layer, 
    //         ptm2c >> cellSize(sx), 
    //         ptm1cn1 >> cellSize(sx));
    //     pack |= state << cellSize(sx);
    // }

    [unroll]
    for (; sx < CELLS_PER_PACK - 2; sx += n) {
        uint state = getStatePackedNCell(
            layer, 
            ptm2c >> cellSize(sx), 
            ptm1cn1 >> cellSize(sx));
        pack |= state << cellSize(sx);
    }

    uint ptm1n2c = appendToPack(ptm1c, ptm1n2, 1);
    pack = pack & cellMask(CELLS_PER_PACK - 2);

    [unroll]
    for (sx = CELLS_PER_PACK - 2; sx < CELLS_PER_PACK; sx += n) {
        uint state = getStatePackedNCell(
            layer, 
            ptm2c >> cellSize(sx),
            ptm1n2c >> cellSize(sx - 2));
        pack |= state << cellSize(sx);
    }

    return pack;
}

uint4 fillPack(uint layer, uint4 ptm2c, uint4 ptm1n2, uint4 ptm1c, uint4 ptm1n1) {
    static const uint n = TABLE_PACK_N;

    uint4 pack = 0;

    uint4 ptm1cn1 = appendToPack(ptm1n1, ptm1c, CELLS_PER_PACK - 1);

    uint sx; 

    [unroll]
    for (sx = 0; sx < CELLS_PER_PACK - 2; sx += n) {
        uint4 state = getStatePackedNCell(
            layer, 
            ptm2c >> cellSize(sx), 
            ptm1cn1 >> cellSize(sx));
        pack |= state << cellSize(sx);
    }

    uint4 ptm1n2c = appendToPack(ptm1c, ptm1n2, 1);
    pack = pack & cellMask(CELLS_PER_PACK - 2);

    [unroll]
    for (sx = CELLS_PER_PACK - 2; sx < CELLS_PER_PACK; sx += n) {
        uint4 state = getStatePackedNCell(
            layer, 
            ptm2c >> cellSize(sx),
            ptm1n2c >> cellSize(sx - 2));
        pack |= state << cellSize(sx);
    }

    return pack;
}

uint4 fillPack(uint layer, uint4 ptm2c, uint ptm1n2, uint4 ptm1c, uint ptm1n1) {
    static const uint n = TABLE_PACK_N;

    uint4 pack = 0;

    uint4 ptm1cn1 = 
        appendToPack(uint4(ptm1n1, ptm1c.xyz), ptm1c, CELLS_PER_PACK - 1);

    uint sx;

    [unroll]
    for (sx = 0; sx < CELLS_PER_PACK - 2; sx += n) {
        uint4 state = getStatePackedNCell(
            layer, 
            ptm2c >> cellSize(sx), 
            ptm1cn1 >> cellSize(sx));
        pack |= state << cellSize(sx);
    }

    pack &= cellMask(CELLS_PER_PACK - 2);
    uint4 ptm1n2c = appendToPack(ptm1c, uint4(ptm1c.yzw, ptm1n2), 1);

    [unroll]
    for (sx = CELLS_PER_PACK - 2; sx < CELLS_PER_PACK; sx += n) {
        uint4 state = getStatePackedNCell(
            layer, 
            ptm2c >> cellSize(sx),
            ptm1n2c >> cellSize(sx - 2));
        pack |= state << cellSize(sx);
    }

    return pack;
}

#endif // CA_RULE_PACKEDNCELL


#endif // CA_RULE_INCLUDED