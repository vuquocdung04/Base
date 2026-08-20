Shader "Custom/IrisWipe"
{
    Properties
    {
        _Radius ("Radius", Range(0, 1)) = 0
        _IsInvert ("Invert", Float) = 0
        _ColorTop ("Color Top", Color) = (0.003921569, 0.5529412, 1, 1)
        _ColorBottom ("Color Bottom", Color) = (0.4745098, 0.9568628, 0.9764706, 1)
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Feather ("Feather", Range(0, 0.05)) = 0.004
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 iris   : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            fixed4 _ColorTop;
            fixed4 _ColorBottom;
            float4 _Center;
            float  _Radius;
            half   _IsInvert;
            float  _Feather;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color  = IN.color;

                float2 aspect = float2(_ScreenParams.x / max(_ScreenParams.y, 1.0), 1.0);
                float2 center = _Center.xy;

                OUT.iris.xy = (IN.texcoord - center) * aspect;
                OUT.iris.z  = _Radius * length(max(center, 1.0 - center) * aspect);

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float dist   = length(IN.iris.xy);
                half  inside = 1.0h - smoothstep(IN.iris.z - _Feather, IN.iris.z + _Feather, dist);

                fixed4 col = lerp(_ColorBottom, _ColorTop, IN.iris.y + _Center.y);

                col.a *= lerp(inside, 1.0h - inside, _IsInvert) * IN.color.a;

                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}
