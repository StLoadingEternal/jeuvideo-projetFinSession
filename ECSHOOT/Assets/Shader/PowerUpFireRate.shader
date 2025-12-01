Shader "Custom/PowerUpFireRate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _Color ("Color", Color) = (1,0,0,1)
        _FireIntensity ("Fire Intensity", Range(0, 5)) = 2
        _NoiseScale ("Noise Scale", Range(0, 10)) = 5
        _Speed ("Speed", Range(0, 10)) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float4 _Color;
            float _FireIntensity;
            float _NoiseScale;
            float _Speed;
            
            // Fonction de bruit simple
            float simpleNoise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Bruit simple pour l'animation
                float noise = simpleNoise(v.uv * _NoiseScale + _Time.y * _Speed);
                v.vertex.y += (noise - 0.5) * 0.1;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Lecture de la texture de bruit
                float2 noiseUV = i.uv * _NoiseScale;
                noiseUV.y += _Time.y * _Speed;
                
                float noise1 = tex2D(_NoiseTex, noiseUV).r;
                float noise2 = tex2D(_NoiseTex, noiseUV * 2.0 + float2(0.2, 0.3)).r;
                
                // Gradient vertical (feu)
                float gradient = 1.0 - i.uv.y;
                gradient = pow(gradient, 2);
                
                // Combinaison pour effet de feu
                float fire = (noise1 * 0.5 + 0.5) * gradient;
                fire += (noise2 * 0.3) * gradient;
                
                // Couleur avec gradient (rouge -> orange -> jaune)
                fixed4 col = _Color;
                
                // Gradient de couleur
                float3 fireColor = lerp(float3(1, 0.5, 0), float3(1, 1, 0), gradient);
                col.rgb = fireColor * fire * _FireIntensity;
                col.a = fire * 0.8 + 0.2;
                
                // Étincelles
                float sparkles = step(0.95, simpleNoise(i.uv * 20 + _Time.y * 5));
                col.rgb += float3(1, 0.8, 0) * sparkles * 0.5;
                
                return col;
            }
            ENDCG
        }
    }
}