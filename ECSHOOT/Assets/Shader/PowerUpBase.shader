Shader "Custom/PowerUpBase"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
        _RotationSpeed ("Rotation Speed", Range(0, 10)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        
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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 normal : NORMAL;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _RotationSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Rotation animée
                float angle = _Time.y * _RotationSpeed;
                float sinRot, cosRot;
                sincos(angle, sinRot, cosRot);
                
                float2 rotatedUV;
                rotatedUV.x = v.uv.x * cosRot - v.uv.y * sinRot;
                rotatedUV.y = v.uv.x * sinRot + v.uv.y * cosRot;
                o.uv = rotatedUV;
                
                // Pulsation
                float pulse = sin(_Time.y * _PulseSpeed) * 0.1 + 1.0;
                v.vertex.xyz *= pulse;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Texture de base
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // Effet de glow radial
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                float glow = (1.0 - dist) * _GlowIntensity;
                
                // Effet de scanline
                float scanline = sin(i.uv.y * 100 + _Time.y * 10) * 0.1 + 0.9;
                
                // Combinaison des effets
                fixed4 col = tex * _Color;
                col.rgb += _GlowColor.rgb * glow;
                col.rgb *= scanline;
                
                // Fresnel effect (bords lumineux)
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - saturate(dot(normalize(i.normal), viewDir)), 2);
                col.rgb += _GlowColor.rgb * fresnel * 0.5;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}