Shader "CoreGame/DarkOverlay"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 0.75)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "DarkOverlay"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings { float4 positionCS : SV_POSITION; };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            // Procedural fullscreen triangle — no mesh needed
            Varyings Vert(uint vertexID : SV_VertexID)
            {
                Varyings o;
                float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
                o.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target { return _Color; }
            ENDHLSL
        }
    }
}
