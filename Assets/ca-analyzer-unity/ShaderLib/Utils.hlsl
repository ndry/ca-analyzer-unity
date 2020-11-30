#ifndef CA_UTILS_INCLUDED
#define CA_UTILS_INCLUDED

static uint rand_seed;
inline uint rand() {
    static const uint mod = 1 << 31;
    static const uint m = 1103515245;
    static const uint i = 12345;
    // random bits 30..0
    return rand_seed = (rand_seed * m + i) % mod;
}

float3 rgb2hsv(float3    c) {
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 hsv2rgb(float3 c) {
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

static const uint TIME_NEIGHBOURHOOD_RADIUS = 2;
static const uint SPACE_NEIGHBOURHOOD_RADIUS = 1;

#endif