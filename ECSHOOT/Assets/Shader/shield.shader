Shader "Custom/ShieldEffect"
{
    Properties
    {
        // Couleurs de base
        _MainColor ("Main Color", Color) = (0.2, 0.6, 1, 0.3)
        _RimColor ("Rim Color", Color) = (0.8, 0.9, 1, 0.8)
        _HitColor ("Hit Color", Color) = (1, 0.2, 0.2, 1)
        _ActivationColor ("Activation Color", Color) = (0, 1, 1, 1)
        
        // Paramètres d'effets
        _RimPower ("Rim Power", Range(0.1, 10)) = 3
        _NoiseScale ("Noise Scale", Range(0, 10)) = 2
        _ScrollSpeed ("Scroll Speed", Range(0, 5)) = 1
        
        // Paramètres dynamiques (contrôlés par script)
        _ShieldHealth ("Shield Health", Range(0, 1)) = 1
        _ActivationProgress ("Activation Progress", Range(0, 1)) = 0
        _HitIntensity ("Hit Intensity", Range(0, 1)) = 0
        _BreakProgress ("Break Progress", Range(0, 1)) = 0
        
        // Textures
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _PatternTex ("Pattern Texture", 2D) = "white" {}
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
            
            sampler2D _NoiseTex;
            sampler2D _PatternTex;
            float4 _NoiseTex_ST;
            float4 _PatternTex_ST;
            
            float4 _MainColor;
            float4 _RimColor;
            float4 _HitColor;
            float4 _ActivationColor;
            
            float _RimPower;
            float _NoiseScale;
            float _ScrollSpeed;
            
            // Paramètres dynamiques
            float _ShieldHealth;
            float _ActivationProgress;
            float _HitIntensity;
            float _BreakProgress;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Distorsion basée sur le bruit et la santé
                float noise = tex2Dlod(_NoiseTex, float4(v.uv * _NoiseScale, 0, 0)).r;
                float distortion = noise * 0.1 * (1 - _ShieldHealth);
                v.vertex.xyz += v.normal * distortion;
                
                // Pulsation basée sur la santé
                float pulse = sin(_Time.y * 2) * 0.05 * _ShieldHealth;
                v.vertex.xyz *= (1 + pulse);
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - o.worldPos);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            // ============ EFFETS VISUELS ============
            
            // Effet d'activation (onde qui part du bas)
            float GetActivationEffect(float3 worldPos)
            {
                // Onde qui monte
                float activationWave = smoothstep(0, 0.3, _ActivationProgress - worldPos.y * 0.5);
                
                // Effet de scan qui suit l'onde
                float scanPos = worldPos.y + _Time.y * _ScrollSpeed;
                float scan = frac(scanPos * 2) * activationWave;
                
                // Bruit pour l'effet énergétique
                float noise = tex2D(_NoiseTex, worldPos.xz * 0.5 + _Time.y * 0.5).r;
                
                return activationWave * (0.5 + noise * 0.5) * scan;
            }
            
            // Effet de hit (ondes concentriques depuis le point d'impact)
            float GetHitEffect(float3 worldPos, float3 hitPoint)
            {
                if (_HitIntensity <= 0) return 0;
                
                float dist = distance(worldPos, hitPoint);
                float wave = sin(dist * 20 - _Time.y * 30) * 0.5 + 0.5;
                
                // Atténuation avec le temps
                float timeSinceHit = _Time.y - _HitIntensity * 10;
                float attenuation = exp(-timeSinceHit * 5);
                
                return wave * _HitIntensity * attenuation;
            }
            
            // Effet de cassure (fissures qui se propagent)
            float GetBreakEffect(float2 uv)
            {
                if (_BreakProgress <= 0) return 0;
                
                // Fissures radiales
                float2 center = float2(0.5, 0.5);
                float angle = atan2(uv.y - center.y, uv.x - center.x);
                float dist = length(uv - center);
                
                // Bruit pour les fissures
                float crackNoise = tex2D(_NoiseTex, uv * 5).r;
                float cracks = step(0.7, crackNoise) * _BreakProgress;
                
                // Effet de fragmentation
                float fragment = sin(dist * 30 + angle * 10 + _Time.y * 5) * 0.5 + 0.5;
                fragment *= _BreakProgress;
                
                return max(cracks, fragment);
            }
            
            // Effet de santé faible (clignotement rapide)
            float GetLowHealthEffect()
            {
                if (_ShieldHealth > 0.3) return 0;
                
                float lowHealthFlash = sin(_Time.y * 10) * 0.5 + 0.5;
                return lowHealthFlash * (1 - _ShieldHealth / 0.3);
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Coordonnées pour les effets
                float2 patternUV = i.uv * _PatternTex_ST.xy + float2(0, _Time.y * _ScrollSpeed);
                
                // Texture de base
                fixed4 pattern = tex2D(_PatternTex, patternUV);
                fixed4 noise = tex2D(_NoiseTex, i.uv * _NoiseScale);
                
                // Effet Fresnel
                float fresnel = pow(1.0 - saturate(dot(normalize(i.normal), i.viewDir)), _RimPower);
                
                // Calculer tous les effets
                float activationEffect = GetActivationEffect(i.worldPos);
                float hitEffect = GetHitEffect(i.worldPos, float3(0,0,0)); // Point d'impact au centre
                float breakEffect = GetBreakEffect(i.uv);
                float lowHealthEffect = GetLowHealthEffect();
                
                // Couleur de base avec santé
                fixed4 col = _MainColor;
                col.rgb *= _ShieldHealth; // Assombrir quand faible santé
                
                // Ajouter l'effet d'activation
                col.rgb += _ActivationColor.rgb * activationEffect * 2;
                
                // Ajouter l'effet de hit
                col.rgb += _HitColor.rgb * hitEffect * 3;
                
                // Ajouter l'effet Fresnel
                col.rgb += _RimColor.rgb * fresnel * (1 + lowHealthEffect);
                
                // Ajouter l'effet de cassure
                col.rgb += float3(1,0,0) * breakEffect * 2;
                
                // Transparence
                col.a = _MainColor.a * _ShieldHealth;
                col.a *= 0.5 + fresnel * 0.5;
                col.a *= 1 - breakEffect * 0.5;
                
                // Effet de scintillement avec le bruit
                col.rgb *= 0.9 + noise.r * 0.2;
                
                // Effet de distorsion visuelle pour les hits
                col.rgb += float3(1,1,1) * hitEffect * 0.5;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}