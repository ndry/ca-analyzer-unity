Shader "Spacetime"
{
    Properties
    {
        _UseGrayShades ("Use Gray Shades", Int) = 0

        _State0_Color0 ("State 0 Main Color", Color) = (0, 0, 0, 1)
        _State1_Color0 ("State 1 Main Color", Color) = (0, 0, 0, 1)
        _State2_Color0 ("State 2 Main Color", Color) = (0, 0, 0, 1)
        _State3_Color0 ("State 3 Main Color", Color) = (0, 0, 0, 1)

        [Header(State 0)]
        [Toggle] _State0_UseGradient ("Gradient", Int) = 0
        [Toggle] _State0_UseScreenCoords ("Use Screen Coords For Gradient", Int) = 1
        [Toggle] _State0_Unstretch ("Unstretch Screen Coords", Int) = 1
        [Toggle] _State0_Radial ("Radial Gradient", Int) = 1
        _State0_Color0 ("Color 0", Color) = (0, 0, 0, 1)
        _State0_Color1 ("Color 1", Color) = (0, 0, 1, 1)
        _State0_Center ("Center", Vector) = (0, 0, 0, 0)
        _State0_Radius ("Radius", Float) = 1
        _State0_Direction ("Direction", Float) = 0
        [Toggle] _State0_DiscreteColors ("Discrete Colors", Int) = 0
        _State0_DiscreteColorsCount ("Discrete Colors Count", Int) = 2
        [Toggle]_State0_InterpolateHSV ("Interpolate In HSV Color Space", Int) = 0

        [Header(State 1)]
        [Toggle] _State1_UseGradient ("Gradient", Int) = 0
        [Toggle] _State1_UseScreenCoords ("Use Screen Coords For Gradient", Int) = 1
        [Toggle] _State1_Unstretch ("Unstretch Screen Coords", Int) = 1
        [Toggle] _State1_Radial ("Radial Gradient", Int) = 1
        _State1_Color0 ("Color 0", Color) = (0, 0, 0, 1)
        _State1_Color1 ("Color 1", Color) = (0, 0, 1, 1)
        _State1_Center ("Center", Vector) = (0, 0, 0, 0)
        _State1_Radius ("Radius", Float) = 1
        _State1_Direction ("Direction", Float) = 0
        [Toggle] _State1_DiscreteColors ("Discrete Colors", Int) = 0
        _State1_DiscreteColorsCount ("Discrete Colors Count", Int) = 2
        [Toggle]_State1_InterpolateHSV ("Interpolate In HSV Color Space", Int) = 0

        [Header(State 2)]
        [Toggle] _State2_UseGradient ("Gradient", Int) = 0
        [Toggle] _State2_UseScreenCoords ("Use Screen Coords For Gradient", Int) = 1
        [Toggle] _State2_Unstretch ("Unstretch Screen Coords", Int) = 1
        [Toggle] _State2_Radial ("Radial Gradient", Int) = 1
        _State2_Color0 ("Color 0", Color) = (0, 0, 0, 1)
        _State2_Color1 ("Color 1", Color) = (0, 0, 1, 1)
        _State2_Center ("Center", Vector) = (0, 0, 0, 0)
        _State2_Radius ("Radius", Float) = 1
        _State2_Direction ("Direction", Float) = 0
        [Toggle] _State2_DiscreteColors ("Discrete Colors", Int) = 0
        _State2_DiscreteColorsCount ("Discrete Colors Count", Int) = 2
        [Toggle]_State2_InterpolateHSV ("Interpolate In HSV Color Space", Int) = 0

        [Header(State 3)]
        [Toggle] _State3_UseGradient ("Gradient", Int) = 0
        [Toggle] _State3_UseScreenCoords ("Use Screen Coords For Gradient", Int) = 1
        [Toggle] _State3_Unstretch ("Unstretch Screen Coords", Int) = 1
        [Toggle] _State3_Radial ("Radial Gradient", Int) = 1
        _State3_Color0 ("Color 0", Color) = (0, 0, 0, 1)
        _State3_Color1 ("Color 1", Color) = (0, 0, 1, 1)
        _State3_Center ("Center", Vector) = (0, 0, 0, 0)
        _State3_Radius ("Radius", Float) = 1
        _State3_Direction ("Direction", Float) = 0
        [Toggle] _State3_DiscreteColors ("Discrete Colors", Int) = 0
        _State3_DiscreteColorsCount ("Discrete Colors Count", Int) = 2
        [Toggle]_State3_InterpolateHSV ("Interpolate In HSV Color Space", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TPACK int
            #define CELL_SIZE_BITS 2
            #define SIZE_OF_PACK_BITS 32
            #define CELLS_PER_PACK (SIZE_OF_PACK_BITS / CELL_SIZE_BITS)
            #define CELL_MASK ((1 << CELL_SIZE_BITS) - 1)
            #define StateVarDef(type, _StateVar) type _State0##_StateVar; type _State1##_StateVar; type _State2##_StateVar; type _State3##_StateVar; 
            #define StateVar(state, _StateVar) (state > 1) ? ((state == 3) ? _State3##_StateVar : _State2##_StateVar) : ((state == 1) ? _State1##_StateVar : _State0##_StateVar)
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 tx : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float2 screenPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                int _UseGrayShades;
                StateVarDef(bool, _UseGradient);
                StateVarDef(bool, _UseScreenCoords);
                StateVarDef(bool, _Unstretch);
                StateVarDef(bool, _Radial);
                StateVarDef(float4, _Color0);
                StateVarDef(float4, _Color1);
                StateVarDef(float2, _Center);
                StateVarDef(float, _Radius);
                StateVarDef(float, _Direction);
                StateVarDef(int, _DiscreteColors);
                StateVarDef(int, _DiscreteColorsCount);
                StateVarDef(int, _InterpolateHSV);

                int _SpaceSize;
                int _TimeSize;
                Buffer<int> _SpacetimePacked;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.tx = v.uv * int2(_TimeSize, _SpaceSize);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            inline TPACK getPack(int t, int xp) {
                static const int spaceSizePacked = (_SpaceSize - 1) / CELLS_PER_PACK + 1;
                return _SpacetimePacked[t * spaceSizePacked + xp];
            }

            inline int getFromPack(TPACK pack, int i) {
                return (pack >> (i * CELL_SIZE_BITS)) & CELL_MASK;
            }

            inline int getState(int2 tx) {
                int t = tx.x;
                int x = tx.y;
                return getFromPack(
                    getPack(t, x / CELLS_PER_PACK), 
                    x % CELLS_PER_PACK);
            }

            float grad(float2 p0, float2 p, float r, float a, bool radial, int discreteColorCount) {
                float2 dp = (p - p0);
                float g;
                if (radial) {
                    g = length(dp) / r;
                    g = floor(g * discreteColorCount) / discreteColorCount;
                    g = sqrt(g);
                } else {
                    float2 direction = float2(cos(a), sin(a));
                    g = dot(dp, direction) / r;
                    g = floor(g * discreteColorCount) / discreteColorCount;
                }
                return clamp(g, 0, 1);
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

            float3 frag (v2f input) : SV_Target
            {
                int state = getState(input.tx);

                if (_UseGrayShades) {
                    return float3(1, 1, 1) * state / (_UseGrayShades - 1);
                }
                
                float2 screenRatio = _ScreenParams.xy / _ScreenParams.yx;
                float2 screenPosUnstretchScale = max(1, screenRatio);

                float g = 0;
                if (StateVar(state, _UseGradient)) {
                    float2 screenPosScale = StateVar(state, _Unstretch) ? screenPosUnstretchScale : 1;
                    float2 screenPos = 0.5 - (0.5 - input.screenPos) * screenPosScale;
                    g = grad(
                        StateVar(state, _Center),
                        StateVar(state, _UseScreenCoords) ? screenPos : input.uv,
                        StateVar(state, _Radius),
                        radians(StateVar(state, _Direction)),
                        StateVar(state, _Radial),
                        StateVar(state, _DiscreteColors) ? StateVar(state, _DiscreteColorsCount) : (256 * 256));
                } 
                if (StateVar(state, _InterpolateHSV)) {
                    float3 c1 = rgb2hsv(StateVar(state, _Color0));
                    float3 c2 = rgb2hsv(StateVar(state, _Color1));
                    if (c1.x - c2.x > 0.5) { c1.x -= 1; }
                    if (c1.x - c2.x < -0.5) { c1.x += 1; }
                    return hsv2rgb(lerp(c1, c2, g));
                } else {
                    return lerp(
                        StateVar(state, _Color0), 
                        StateVar(state, _Color1), 
                        g);
                }
            }
            ENDCG
        }
    }
}
