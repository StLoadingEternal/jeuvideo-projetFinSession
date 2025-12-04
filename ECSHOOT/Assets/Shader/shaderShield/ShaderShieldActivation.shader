Shader "Custom/ShieldActivationWave"
{
    Properties
    {
        _Color ("Wave Color", Color) = (0, 1, 1, 1)
        _WaveWidth ("Wave Width", Range(0, 1)) = 0.1
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 5
        _WaveCount ("Wave Count", Range(1, 10)) = 3
        _NoiseAmount ("Noise Amount", Range(0, 1)) = 0.3
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent+100"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        Blend One One
        ZWrite Off
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };
            
            float4 _Color;
            float _WaveWidth;
            float _WaveSpeed;
            float _WaveCount;
            float _NoiseAmount;
            
            // Variables contrôlées par script
            uniform float _ActivationTime;
            uniform float3 _ActivationCenter;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - o.worldPos);
                return o;
            }
            
            // Fonction de bruit
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.worldPos, _ActivationCenter);
                
                // Onde principale
                float wave = 0;
                for (int w = 0; w < _WaveCount; w++)
                {
                    float waveOffset = w * 0.3;
                    float wavePos = dist - (_ActivationTime + waveOffset) * _WaveSpeed;
                    wave += smoothstep(_WaveWidth, 0, abs(wavePos));
                }
                
                // Bruit pour l'effet d'énergie
                float noise = hash(i.worldPos.xz * 0.5 + _ActivationTime);
                wave *= 1 + noise * _NoiseAmount;
                
                // Atténuation avec la distance
                float attenuation = 1 - smoothstep(0, 10, dist);
                wave *= attenuation;
                
                // Fresnel pour les bords
                float3 normal = normalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
                float fresnel = pow(1 - abs(dot(normal, i.viewDir)), 3);
                
                fixed4 col = _Color;
                col.a *= wave * fresnel;
                col.rgb *= wave * 2;
                
                return col;
            }
            ENDCG
        }
    }
}