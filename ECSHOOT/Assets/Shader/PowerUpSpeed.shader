Shader "Custom/PowerUpSpeed"
{
    Properties
    {
        _Color ("Color", Color) = (0,1,0,1) // Vert pour Speed
        _SpeedLines ("Speed Lines", Range(0, 1)) = 0.5
        _TrailLength ("Trail Length", Range(0, 10)) = 3
        _Distortion ("Distortion", Range(0, 1)) = 0.3
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
                float4 screenPos : TEXCOORD1;
            };
            
            float4 _Color;
            float _SpeedLines;
            float _TrailLength;
            float _Distortion;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Effet de vitesse (étirement)
                v.vertex.x += sin(v.uv.y * 10 + _Time.y * 5) * 0.1 * _Distortion;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Lignes de vitesse
                float speedLines = sin(i.uv.x * 50 + _Time.y * 20);
                speedLines = step(0.9, abs(speedLines)) * _SpeedLines;
                
                // Effet de traînée
                float trail = 1.0 - frac(i.uv.x * _TrailLength + _Time.y * 2);
                trail = pow(trail, 2);
                
                // Effet de distortion temporelle
                float timeDistortion = sin(_Time.y * 3 + i.uv.x * 10) * 0.1;
                
                // Couleur avec effet
                fixed4 col = _Color;
                col.rgb *= 0.8 + trail * 0.5;
                col.rgb += float3(1, 1, 0.5) * speedLines;
                col.a = 0.7 + trail * 0.3 + timeDistortion * 0.2;
                
                // Effet de scan radial
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                float radialScan = sin(dist * 20 - _Time.y * 10) * 0.5 + 0.5;
                col.rgb += float3(0.5, 1, 0.5) * radialScan * 0.3;
                
                return col;
            }
            ENDCG
        }
    }
}