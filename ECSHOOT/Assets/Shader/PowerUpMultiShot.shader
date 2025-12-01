Shader "Custom/PowerUpMultiShot"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,1,1) // Bleu pour Multi-Shot
        _HologramIntensity ("Intensity", Range(0, 5)) = 1
        _ScanSpeed ("Scan Speed", Range(0, 10)) = 2
        _GridSize ("Grid Size", Range(1, 50)) = 10
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
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
            };
            
            float4 _Color;
            float _HologramIntensity;
            float _ScanSpeed;
            float _GridSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Effet de grille holographique
                float2 grid = frac(i.uv * _GridSize);
                float gridLines = step(0.95, grid.x) + step(0.95, grid.y);
                
                // Ligne de scan qui descend
                float scanLine = step(i.uv.y, frac(_Time.y * _ScanSpeed));
                scanLine *= 1.0 - i.uv.y;
                
                // Fresnel pour les bords
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - saturate(dot(normalize(i.normal), viewDir)), 3);
                
                // Combinaison
                fixed4 col = _Color;
                col.a = 0.3 + fresnel * 0.5;
                col.rgb *= _HologramIntensity;
                col.rgb += float3(0.5, 0.5, 1) * gridLines * 0.5;
                col.rgb += float3(0, 1, 1) * scanLine * 2;
                
                // Effet de distorsion
                float distortion = sin(i.uv.x * 20 + _Time.y * 5) * 0.01;
                col.rgb *= 1.0 + distortion;
                
                return col;
            }
            ENDCG
        }
    }
}