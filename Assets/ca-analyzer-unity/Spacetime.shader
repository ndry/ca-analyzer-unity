Shader "Spacetime"
{
    Properties
    {
        _StateColor0 ("Color For State 0", Color) = (0, 0, 0, 1)
        _StateColor1 ("Color For State 1", Color) = (0, 0, 1, 1)
        _StateColor2 ("Color For State 2", Color) = (0, 1, 0, 1)
        _StateColor3 ("Color For State 3", Color) = (1, 0, 0, 1)
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

            #define CELL_SIZE_BITS 2

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _StateColor0;
                float4 _StateColor1;
                float4 _StateColor2;
                float4 _StateColor3;
                int _SpaceSize;
                int _TimeSize;
                Buffer<int> _SpacetimePacked;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f input) : SV_Target
            {
                float4 colors[4] = {_StateColor0, _StateColor1, _StateColor2, _StateColor3};
                int2 tx = input.uv * int2(_TimeSize, _SpaceSize);
                int t = tx.x;
                int x = tx.y;
                int sizeOfPackBits = 32;
                int cellsPerPack = sizeOfPackBits / CELL_SIZE_BITS;
                int spaceSizePacked = (_SpaceSize - 1) / cellsPerPack + 1;
                int pack = _SpacetimePacked[t * spaceSizePacked + x / cellsPerPack];
                int state = (pack >> ((x % cellsPerPack) * CELL_SIZE_BITS)) & 0x3;
                return colors[state];
            }
            ENDCG
        }
    }
}
