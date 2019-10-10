Shader "Unlit/Particle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Size ("Size", Float) = 3.0
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

            struct Particle
            {
                float3 Position;
                float3 OutPosition;
                float Scale;
                float2 UV;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float psize : PSIZE;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Size;

            StructuredBuffer<Particle> _Particles;

            v2f vert (uint id: SV_VertexID)
            {
                Particle p = _Particles[id];

                v2f o;
                o.vertex = UnityObjectToClipPos(p.OutPosition);
                o.uv = p.UV;
                o.psize = _Size * p.Scale;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col * _Color;
            }
            ENDCG
        }
    }
}
