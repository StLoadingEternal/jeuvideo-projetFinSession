Shader "Custom/ShieldBreak"
{
    Properties
    {
        _Color ("Break Color", Color) = (1, 0, 0, 1)
        _CrackWidth ("Crack Width", Range(0, 0.1)) = 0.02
        _FragmentSize ("Fragment Size", Range(0, 1)) = 0.1
        _BreakSpeed ("Break Speed", Range(0, 10)) = 2
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent+200"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        Blend One One
        ZWrite Off
        Cull Off
        
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
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normal : NORMAL;
            };
            
            float4 _Color;
            float _CrackWidth;
            float _FragmentSize;
            float _BreakSpeed;
            
            // Contrôlé par script
            uniform float _BreakTime;
            uniform float _BreakProgress;
            
            // Fonction de bruit pour les fissures
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Animation de fragmentation
                float breakTime = _Time.y - _BreakTime;
                float fragmentOffset = hash(v.uv) * _FragmentSize * breakTime * _BreakSpeed;
                v.vertex.xyz += v.normal * fragmentOffset * _BreakProgress;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                if (_BreakProgress <= 0) return 0;
                
                // Fissures radiales depuis le centre
                float2 center = float2(0.5, 0.5);
                float angle = atan2(i.uv.y - center.y, i.uv.x - center.x);
                float dist = length(i.uv - center);
                
                // Créer des fissures
                float crack = 0;
                for (int c = 0; c < 8; c++)
                {
                    float crackAngle = c * 3.14159 / 4;
                    float crackDist = abs(angle - crackAngle);
                    if (crackDist < 0.1)
                    {
                        crack += smoothstep(_CrackWidth, 0, abs(dist - 0.5));
                    }
                }
                
                // Effet de fragmentation (pixels qui s'éloignent)
                float fragment = hash(i.uv * 10) * _BreakProgress;
                
                // Effet d'explosion
                float explosion = smoothstep(0, 1, _BreakProgress - dist);
                
                // Combiner tous les effets
                float breakEffect = max(crack, max(fragment, explosion)) * _BreakProgress;
                
                // Clignotement
                float flicker = sin(_Time.y * 20) * 0.5 + 0.5;
                breakEffect *= flicker;
                
                fixed4 col = _Color;
                col.a *= breakEffect;
                col.rgb *= breakEffect * 2;
                
                return col;
            }
            ENDCG
        }
    }
}