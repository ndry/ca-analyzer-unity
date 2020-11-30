#ifndef CA_PACKER_INCLUDED
#define CA_PACKER_INCLUDED

#define STATE_COUNT 3

#define TPACK uint
#define TPACK1 TPACK##1
#define TPACK2 TPACK##2
#define TPACK3 TPACK##3
#define TPACK4 TPACK##4
#define CELL_SIZE_BITS 2
#define SIZE_OF_PACK_BITS 32
#define CELLS_PER_PACK (SIZE_OF_PACK_BITS / CELL_SIZE_BITS)
#define CELL_MASK ~(-1 << CELL_SIZE_BITS)

inline uint cellSize(uint cellCount) {
    return cellCount * CELL_SIZE_BITS;
}
inline uint4 cellSize(uint4 cellCount) {
    return cellCount * CELL_SIZE_BITS;
}
inline uint cellMask(uint cellCount) {
    return ~(-1 << cellSize(cellCount));
}
inline uint4 cellMask(uint4 cellCount) {
    return ~(-1 << cellSize(cellCount));
}

inline TPACK shlCell(TPACK pack, uint cellCount) {
    return pack << cellSize(cellCount);
}
inline TPACK4 shlCell(TPACK4 pack, uint cellCount) {
    return pack << cellSize(cellCount);
}
inline TPACK4 shlCell(TPACK4 pack, uint4 cellCount) {
    return pack << cellSize(cellCount);
}
 
inline TPACK shrCell(TPACK pack, uint cellCount) {
    return pack >> cellSize(cellCount);
}
inline TPACK4 shrCell(TPACK4 pack, uint cellCount) {
    return pack >> cellSize(cellCount);
}
inline TPACK4 shrCell(TPACK4 pack, uint4 cellCount) {
    return pack >> cellSize(cellCount);
}

inline uint getFromPack(TPACK pack, uint i) {
    return (pack >> cellSize(i)) & CELL_MASK;
}
inline uint4 getFromPack(TPACK pack, uint4 i) {
    return (pack >> cellSize(i)) & CELL_MASK;
}

inline uint setIntoPack(TPACK pack, uint i, uint value) {
    return pack = pack & ~(CELL_MASK << cellSize(i)) | (value << cellSize(i));
}

inline uint setIntoPackDirty(TPACK pack, uint i, uint value) {
    return pack = pack | (value << cellSize(i));
}

inline TPACK appendToPack(TPACK pack, uint data, uint dataCellCount) {
    uint shift = dataCellCount * CELL_SIZE_BITS;
    uint lastShift = (CELLS_PER_PACK - dataCellCount) * CELL_SIZE_BITS;
    uint lastMask = (1 << lastShift) - 1;

    return((pack >> shift) & lastMask) | (data << lastShift);
}

inline TPACK4 appendToPack(TPACK4 pack, uint4 data, uint4 dataCellCount) {
    uint4 shift = dataCellCount * CELL_SIZE_BITS;
    uint4 lastShift = (CELLS_PER_PACK - dataCellCount) * CELL_SIZE_BITS;
    uint4 lastMask = (1 << lastShift) - 1;

    return((pack >> shift) & lastMask) | (data << lastShift);
}

inline TPACK4 appendToPack(TPACK4 pack, uint4 data, uint dataCellCount) {
    uint shift = cellSize(dataCellCount);
    uint lastShift = (CELLS_PER_PACK - dataCellCount) * CELL_SIZE_BITS;
    uint lastMask = (1 << lastShift) - 1;
    
    return((pack >> shift) & lastMask) | (data << lastShift);
}

#endif