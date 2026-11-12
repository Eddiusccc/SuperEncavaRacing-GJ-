Shader "Skybox/BlendTwoCubemaps"
{
    Properties
    {
        [NoScaleOffset] _DayTex ("Day Cubemap", Cube) = "" {}
        [NoScaleOffset] _NightTex ("Night Cubemap", Cube) = "" {}
        _Blend ("Blend", Range(0, 1)) = 0
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Exposure ("Exposure", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            samplerCUBE _DayTex;
            samplerCUBE _NightTex;
            float _Blend;
            float4 _Tint;
            float _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float4 dayCol = texCUBE(_DayTex, dir);
                float4 nightCol = texCUBE(_NightTex, dir);
                float3 col = lerp(dayCol.rgb, nightCol.rgb, saturate(_Blend));
                col *= _Tint.rgb;
                col *= _Exposure;
                return float4(col, 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
