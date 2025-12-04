Shader "Custom/ShieldHitRipple"
{
    Properties
    {
        _Color ("Ripple Color", Color) = (1, 0.2, 0.2, 1)
        _RippleSpeed ("Ripple Speed", Range(0, 20)) = 10
        _RippleCount ("Ripple Count", Range(1, 10)) = 3
        _RippleWidth ("Ripple Width", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent+150"
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
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            float4 _Color;
            float _RippleSpeed;
            float _RippleCount;
            float _RippleWidth;
            
            // Contrôlé par script
            uniform float3 _HitPosition;
            uniform float _HitTime;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.worldPos, _HitPosition);
                float timeSinceHit = _Time.y - _HitTime;
                
                if (timeSinceHit < 0 || timeSinceHit > 1) return 0;
                
                // Ondes concentriques
                float ripple = 0;
                for (int r = 0; r < _RippleCount; r++)
                {
                    float rippleOffset = r * 0.2;
                    float ripplePos = dist - (timeSinceHit + rippleOffset) * _RippleSpeed;
                    ripple += smoothstep(_RippleWidth, 0, abs(ripplePos));
                }
                
                // Atténuation avec le temps
                float attenuation = 1 - timeSinceHit;
                ripple *= attenuation * attenuation; // Quadratic falloff
                
                // Atténuation avec la distance
                float distanceAttenuation = 1 - smoothstep(0, 5, dist);
                ripple *= distanceAttenuation;
                
                fixed4 col = _Color;
                col.a *= ripple;
                col.rgb *= ripple * 3;
                
                return col;
            }
            ENDCG
        }
    }
}