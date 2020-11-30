#include "ShaderLib/Utils.hlsl"
#include "ShaderLib/Packer.hlsl"
#include "ShaderLib/SpacetimePacked.hlsl"

int _Layer;

void getState_float(float2 uv, out float ret) {
    uint t = uv.x * _TimeSize;
    uint x = uv.y * _SpaceSize;
    ret = getFromPack(
        getPack(_Layer, t, x / CELLS_PER_PACK), 
        x % CELLS_PER_PACK);
}