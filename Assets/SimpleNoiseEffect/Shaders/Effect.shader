Shader "Unlit/Appear"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PointSize ("Point size", Float) = 0.3
        _Intensity ("Intensity", Float) = 1.0
        _Height ("Height", Float) = 1.5
        _Fit ("Fit", Range(0, 1)) = 0.0
        _OffsetY ("Offset Y", Float) = 2.0
        _Rotate ("Rotate", Float) = 10.0
        _Scale ("Scale", Float) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./NoiseMath.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float size : PSIZE;
                float alpha : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _PointSize;
            fixed _Intensity;
            fixed _Height;
            fixed _Fit;
            fixed _OffsetY;
            fixed _Rotate;
            fixed _Scale;

            static const float PI = 3.14159265;
            static const float PI2 = PI * 2.0;

            float rand(float x)
            {
                return frac(sin(x) * 43758.5453);
            }

            float2x2 rot(float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float2x2(c, -s, s, c);
            }

            v2f vert (appdata v)
            {
                v2f o;

                float3 pos = v.vertex.xyz;
                float sc = 1.0 + _Scale * _Fit;

                pos.xz = mul(pos.xz, float2x2(sc, 0, 0, sc));
                pos.xz = mul(pos.xz, rot(_Fit * PI2 * _Rotate));
                pos = CurlNoise(pos * 0.3);

                v.vertex.xyz += (pos * 2.0 - 1.0) * _Fit;
                v.vertex.y += _Fit * _OffsetY;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.size = _PointSize;
                o.alpha = 1.0 - _Fit;

                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a = i.alpha;
                return col;
            }
            ENDCG
        }
    }
}
