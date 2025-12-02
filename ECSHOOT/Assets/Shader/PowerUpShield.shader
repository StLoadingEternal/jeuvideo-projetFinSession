Shader "Custom/PowerUpShield"

{

    Properties

    {

        // Couleurs principales

        _MainColor ("Main Color", Color) = (0.3, 0.7, 1, 0.8)

        _CoreColor ("Core Color", Color) = (0.8, 0.9, 1, 1)

        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)

        // Effets

        _RimPower ("Rim Power", Range(0.1, 10)) = 3

        _SpinSpeed ("Spin Speed", Range(0, 10)) = 2

        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1

        _FloatSpeed ("Float Speed", Range(0, 5)) = 1

        _NoiseScale ("Noise Scale", Range(0, 10)) = 3

        // Forme géométrique

        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.3

        _OuterRadius ("Outer Radius", Range(0, 1)) = 0.5

        // Hologramme

        _HoloLines ("Holo Lines", Range(1, 50)) = 10

        _HoloSpeed ("Holo Speed", Range(0, 5)) = 1

    }

    SubShader

    {

        Tags 

        { 

            "Queue" = "Transparent"

            "RenderType" = "Transparent"

            "IgnoreProjector" = "True"

        }

        Blend SrcAlpha OneMinusSrcAlpha

        ZWrite Off

        Cull Back

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

                float3 normal : NORMAL;

                float2 uv : TEXCOORD0;

            };

            struct v2f

            {

                float4 pos : SV_POSITION;

                float2 uv : TEXCOORD0;

                float3 worldPos : TEXCOORD1;

                float3 normal : TEXCOORD2;

                float3 viewDir : TEXCOORD3;

                UNITY_FOG_COORDS(4)

            };

            // Properties

            float4 _MainColor;

            float4 _CoreColor;

            float4 _RimColor;

            float _RimPower;

            float _SpinSpeed;

            float _PulseSpeed;

            float _FloatSpeed;

            float _NoiseScale;

            float _InnerRadius;

            float _OuterRadius;

            float _HoloLines;

            float _HoloSpeed;

            // Fonction de bruit simple

            float noise(float2 p)

            {

                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);

            }

            v2f vert(appdata v)

            {

                v2f o;

                // Animation de rotation

                float angle = _Time.y * _SpinSpeed;

                float sinRot, cosRot;

                sincos(angle, sinRot, cosRot);

                // Rotation autour de l'axe Y

                float3 rotatedPos;

                rotatedPos.x = v.vertex.x * cosRot - v.vertex.z * sinRot;

                rotatedPos.z = v.vertex.x * sinRot + v.vertex.z * cosRot;

                rotatedPos.y = v.vertex.y;

                // Animation de pulsation

                float pulse = sin(_Time.y * _PulseSpeed) * 0.1 + 1.0;

                rotatedPos *= pulse;

                // Flottement vertical

                rotatedPos.y += sin(_Time.y * _FloatSpeed + v.vertex.x) * 0.1;

                o.pos = UnityObjectToClipPos(float4(rotatedPos, 1));

                o.worldPos = mul(unity_ObjectToWorld, float4(rotatedPos, 1)).xyz;

                o.normal = UnityObjectToWorldNormal(v.normal);

                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - o.worldPos);

                o.uv = v.uv;

                UNITY_TRANSFER_FOG(o, o.pos);

                return o;

            }

            fixed4 frag(v2f i) : SV_Target

            {

                // Coordonnées polaires pour effets circulaires

                float2 center = float2(0.5, 0.5);

                float2 dir = i.uv - center;

                float distance = length(dir);

                float angle = atan2(dir.y, dir.x);

                // Effet Fresnel (bords lumineux)

                float fresnel = pow(1.0 - saturate(dot(normalize(i.normal), i.viewDir)), _RimPower);

                // Cercle intérieur (core)

                float innerCircle = smoothstep(_InnerRadius - 0.05, _InnerRadius + 0.05, distance);

                float outerCircle = smoothstep(_OuterRadius - 0.05, _OuterRadius + 0.05, distance);

                float ring = innerCircle * (1 - outerCircle);

                // Effets holographiques

                float holoScan = sin(angle * _HoloLines + _Time.y * _HoloSpeed);

                float holoLines = step(0.9, abs(holoScan)) * ring;

                // Spirales énergétiques

                float spiral = sin(angle * 10 + distance * 20 - _Time.y * 3);

                float spiralLines = step(0.95, spiral) * (1 - ring);

                // Particules tournoyantes

                float particles = step(0.99, noise(float2(angle * 5, _Time.y * 2))) * (1 - ring);

                // Combinaison des couleurs

                fixed4 col = _MainColor;

                // Cœur brillant

                col.rgb = lerp(col.rgb, _CoreColor, (1 - innerCircle) * 2);

                // Bords lumineux

                col.rgb += _RimColor.rgb * fresnel * 2;

                // Lignes holographiques

                col.rgb += float3(0.5, 0.8, 1) * holoLines * 0.8;

                // Spirales

                col.rgb += float3(0.3, 0.7, 1) * spiralLines * 0.6;

                // Particules

                col.rgb += float3(1, 1, 1) * particles;

                // Transparence

                col.a = _MainColor.a * (0.3 + ring * 0.7 + fresnel * 0.3);

                col.a *= 0.7 + sin(_Time.y * 2) * 0.3; // Clignotement doux

                // Effet de distorsion de l'air (heat haze)

                float heatHaze = sin(i.worldPos.x * 5 + _Time.y * 3) * 0.01;

                col.rgb *= 1 + heatHaze;

                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;

            }

            ENDCG

        }

    }

    FallBack "Transparent/Diffuse"

}
 