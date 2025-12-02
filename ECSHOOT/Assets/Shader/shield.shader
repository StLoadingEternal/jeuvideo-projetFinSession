Shader "Custom/ShieldEffect"

{

    Properties

    {

        // Couleurs

        _MainColor ("Main Color", Color) = (0.2, 0.6, 1, 0.3)

        _RimColor ("Rim Color", Color) = (0.8, 0.9, 1, 0.8)

        _EmissionColor ("Emission Color", Color) = (0.3, 0.7, 1, 1)

        // Effets

        _RimPower ("Rim Power", Range(0.1, 10)) = 3

        _NoiseScale ("Noise Scale", Range(0, 10)) = 2

        _ScrollSpeed ("Scroll Speed", Range(0, 5)) = 1

        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1

        _PulseAmplitude ("Pulse Amplitude", Range(0, 1)) = 0.2

        // Tex

        _NoiseTex ("Noise Texture", 2D) = "white" {}

        _NormalMap ("Normal Map", 2D) = "bump" {}

        // Distortion

        _DistortionAmount ("Distortion Amount", Range(0, 0.1)) = 0.02

    }

    SubShader

    {

        Tags 

        { 

            "Queue" = "Transparent"

            "RenderType" = "Transparent"

            "IgnoreProjector" = "True"

        }

        // Premier pass : effet de distorsion

        GrabPass

        {

            "_GrabTexture"

        }

        // Deuxième pass : rendu du bouclier

        Pass

        {

            Name "SHIELD"

            Tags { "LightMode" = "ForwardBase" }

            Blend SrcAlpha OneMinusSrcAlpha

            ZWrite Off

            Cull Back

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

                float4 tangent : TANGENT;

            };

            struct v2f

            {

                float4 pos : SV_POSITION;

                float2 uv : TEXCOORD0;

                float3 worldPos : TEXCOORD1;

                float3 normal : TEXCOORD2;

                float3 viewDir : TEXCOORD3;

                float4 grabPos : TEXCOORD4;

                float2 uvNoise : TEXCOORD5;

                UNITY_FOG_COORDS(6)

            };

            // Properties

            sampler2D _NoiseTex;

            sampler2D _NormalMap;

            sampler2D _GrabTexture;

            float4 _NoiseTex_ST;

            float4 _NormalMap_ST;

            float4 _MainColor;

            float4 _RimColor;

            float4 _EmissionColor;

            float _RimPower;

            float _NoiseScale;

            float _ScrollSpeed;

            float _PulseSpeed;

            float _PulseAmplitude;

            float _DistortionAmount;

            // Fonction de bruit simple

            float2 hash(float2 p)

            {

                p = float2(dot(p, float2(127.1, 311.7)),

                          dot(p, float2(269.5, 183.3)));

                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);

            }

            float noise(float2 p)

            {

                float2 i = floor(p);

                float2 f = frac(p);

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(dot(hash(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),

                                dot(hash(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),

                            lerp(dot(hash(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),

                                dot(hash(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);

            }

            v2f vert(appdata v)

            {

                v2f o;

                // Animation de pulsation

                float pulse = sin(_Time.y * _PulseSpeed) * _PulseAmplitude + 1.0;
v.vertex.xyz *= pulse;

                // Animation de bruit sur les vertices

                float vertexNoise = noise(v.uv * _NoiseScale + _Time.y * _ScrollSpeed) * 0.1;
v.vertex.xyz += v.normal * vertexNoise;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                o.normal = UnityObjectToWorldNormal(v.normal);

                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - o.worldPos);

                o.uv = v.uv;

                o.uvNoise = v.uv * _NoiseScale + _Time.y * _ScrollSpeed;

                // Position pour le GrabPass

                o.grabPos = ComputeGrabScreenPos(o.pos);

                // Distortion basée sur la normale

                float3 tangent = UnityObjectToWorldDir(v.tangent.xyz);

                float3 bitangent = cross(o.normal, tangent) * v.tangent.w;

                float3x3 TBN = float3x3(tangent, bitangent, o.normal);

                float3 normalTex = UnpackNormal(tex2Dlod(_NormalMap, float4(o.uv * _NormalMap_ST.xy, 0, 0)));

                float3 worldNormal = mul(TBN, normalTex);

                // Appliquer la distortion au GrabPos

                float2 distortion = worldNormal.xy * _DistortionAmount;

                o.grabPos.xy += distortion;

                UNITY_TRANSFER_FOG(o, o.pos);

                return o;

            }

            fixed4 frag(v2f i) : SV_Target

            {

                // Lecture de la texture de bruit

                float noise1 = tex2D(_NoiseTex, i.uvNoise).r;

                float noise2 = tex2D(_NoiseTex, i.uvNoise * 1.7 + float2(0.2, 0.3)).r;

                // Effet Fresnel (bords lumineux)

                float fresnel = pow(1.0 - saturate(dot(normalize(i.normal), i.viewDir)), _RimPower);

                // Pattern de scan qui tourne

                float2 center = float2(0.5, 0.5);

                float2 dir = normalize(i.uv - center);

                float angle = atan2(dir.y, dir.x);

                float scan = sin(angle * 10 + _Time.y * 3) * 0.5 + 0.5;

                // Texture Grab avec distortion

                fixed4 grabColor = tex2Dproj(_GrabTexture, i.grabPos);

                // Combinaison des effets

                fixed4 col = _MainColor;

                // Ajouter le rim effect

                col.rgb += _RimColor.rgb * fresnel * 2;

                // Ajouter l'émission

                col.rgb += _EmissionColor.rgb * (noise1 * 0.5 + 0.5) * 0.3;

                // Ajouter le scan effect

                col.rgb += float3(0.5, 0.8, 1) * scan * 0.2;

                // Transparence avec bruit

                col.a = _MainColor.a * (0.6 + fresnel * 0.4);

                col.a *= 0.8 + noise1 * 0.2;

                // Appliquer la distortion sur la couleur de fond

                col.rgb = lerp(grabColor.rgb, col.rgb, col.a * 0.7);

                // Effet de grille énergétique

                float grid1 = step(0.95, frac(i.uv.x * 10 + noise1 * 0.1));

                float grid2 = step(0.95, frac(i.uv.y * 10 + noise2 * 0.1));

                float grid = max(grid1, grid2);

                col.rgb += float3(0.3, 0.8, 1) * grid * 0.5;

                // Particules d'énergie flottantes

                float particles = step(0.98, noise1) * step(noise2, 0.02);

                col.rgb += float3(1, 1, 1) * particles;

                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;

            }

            ENDCG

        }

    }

    FallBack "Transparent/Diffuse"

}
 