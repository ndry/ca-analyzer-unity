#ifndef CA_SPACETIMEPACKED_INCLUDED
#define CA_SPACETIMEPACKED_INCLUDED

#include "Utils.hlsl"
#include "Packer.hlsl"

#ifdef CA_SPACETIMEPACKED_READWRITE
RWBuffer<int> _SpacetimePacked;
#else
Buffer<int> _SpacetimePacked;
#endif
uint _SpaceSize;
uint _TimeSize;

static const uint spaceSizePacked = (_SpaceSize - 1) / CELLS_PER_PACK + 1;
static const uint layerSize = spaceSizePacked * _TimeSize;
uint spacetimePacked_layer;
uint spacetimePacked_layerOffset;

void spacetimePacked_setLayer(uint layer) {
    spacetimePacked_layer = layer;
    spacetimePacked_layerOffset = layer * layerSize;
}
inline uint spacetimePacked_getPackIndex(
    uint layer, uint t, uint xp
) {
    return layer * layerSize
        + t * spaceSizePacked 
        + xp;
}
inline uint spacetimePacked_getPackIndex(
    uint t, uint xp
) {
    return spacetimePacked_layerOffset
        + t * spaceSizePacked 
        + xp;
}

inline TPACK getPack(int layer, int t, int xp) {
    return _SpacetimePacked[
        spacetimePacked_getPackIndex(layer, t, xp)];
}
inline TPACK getPack(int t, int xp) {
    return _SpacetimePacked[
        spacetimePacked_getPackIndex(t, xp)];
}

#ifdef CA_SPACETIMEPACKED_READWRITE
inline void setPack(int layer, int t, int xp, TPACK value) {
    _SpacetimePacked[
        spacetimePacked_getPackIndex(layer, t, xp)
    ] = value;
}
inline void setPack(int t, int xp, TPACK value) {
    _SpacetimePacked[
        spacetimePacked_getPackIndex(t, xp)
    ] = value;
}
#endif

#endif