Shader "Custom/Particles/ShieldActivateShader"

{

    Properties

    {

        _MainTex ("Particle Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)

        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2

    }

    SubShader

    {

        Tags 

        { 

            "Queue"="Transparent"

            "IgnoreProjector"="True"

            "RenderType"="Transparent"

            "PreviewType"="Plane"

        }

        Blend SrcAlpha One

        ColorMask RGB

        Cull Back

        ZWrite Off

        Lighting Off

        Pass

        {

            CGPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata

            {

                float4 vertex : POSITION;

                float4 color : COLOR;

                float2 texcoord : TEXCOORD0;

            };

            struct v2f

            {

                float4 vertex : SV_POSITION;

                float4 color : COLOR;

                float2 texcoord : TEXCOORD0;

                UNITY_FOG_COORDS(1)

            };

            sampler2D _MainTex;

            float4 _MainTex_ST;

            float4 _Color;

            float _GlowIntensity;

            v2f vert(appdata v)

            {

                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.color = v.color * _Color;

                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                UNITY_TRANSFER_FOG(o, o.vertex);

                return o;

            }

            fixed4 frag(v2f i) : SV_Target

            {

                fixed4 tex = tex2D(_MainTex, i.texcoord);

                fixed4 col = tex * i.color;

                col.rgb *= _GlowIntensity;

                UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0));

                return col;

            }

            ENDCG

        }

    }

}
 