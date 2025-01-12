Shader "Custom/Pristine Grid with Transparency"
{
    Properties
    {
        [KeywordEnum(MeshUV, WorldX, WorldY, WorldZ)] _UVMode("UV Mode", Float) = 2.0
        _GridScale("Grid Scale", Float) = 1.0

        _LineWidthX("Line Width X", Range(0,1.0)) = 0.01
        _LineWidthY("Line Width Y", Range(0,1.0)) = 0.01

        _LineColor("Line Color", Color) = (1,1,1,1)
    }
        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma shader_feature _ _UVMODE_MESHUV _UVMODE_WORLDX _UVMODE_WORLDZ // _UVMODE_WORLDY is default

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float _GridScale;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

            #if defined(_UVMODE_MESHUV)
                o.uv = v.uv * _GridScale;
            #else
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                float3 cameraCenteringOffset = floor(_WorldSpaceCameraPos * _GridScale);
                float3 cameraSnappedWorldPos = worldPos * _GridScale - cameraCenteringOffset;

            #if defined(_UVMODE_WORLDX)
                o.uv = cameraSnappedWorldPos.xy;
            #elif defined(_UVMODE_WORLDZ)
                o.uv = cameraSnappedWorldPos.yz;
            #else // defined(_UVMODE_WORLDY)
                o.uv = cameraSnappedWorldPos.xz;
            #endif
            #endif

                return o;
            }

            float PristineGrid(float2 uv, float2 lineWidth)
            {
                lineWidth = saturate(lineWidth);
                float4 uvDDXY = float4(ddx(uv), ddy(uv));
                float2 uvDeriv = float2(length(uvDDXY.xz), length(uvDDXY.yw));
                bool2 invertLine = lineWidth > 0.5;
                float2 targetWidth = invertLine ? 1.0 - lineWidth : lineWidth;
                float2 drawWidth = clamp(targetWidth, uvDeriv, 0.5);
                float2 lineAA = max(uvDeriv, 0.000001) * 1.5;
                float2 gridUV = abs(frac(uv) * 2.0 - 1.0);
                gridUV = invertLine ? gridUV : 1.0 - gridUV;
                float2 grid2 = smoothstep(drawWidth + lineAA, drawWidth - lineAA, gridUV);
                grid2 *= saturate(targetWidth / drawWidth);
                grid2 = lerp(grid2, targetWidth, saturate(uvDeriv * 2.0 - 1.0));
                grid2 = invertLine ? 1.0 - grid2 : grid2;
                return lerp(grid2.x, 1.0, grid2.y);
            }

            float _LineWidthX, _LineWidthY;
            half4 _LineColor;

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate mask to only apply to top-facing surfaces
                float3 worldUp = float3(0, 1, 0);
                float surfaceFacingUp = saturate(dot(i.worldNormal, worldUp));

                float grid = PristineGrid(i.uv, float2(_LineWidthX, _LineWidthY));

                // Use line color for the grid, make the base transparent
                return fixed4(_LineColor.rgb, grid * _LineColor.a * surfaceFacingUp);
            }
            ENDCG
        }
    }
}
