Shader "Custom/FixedWorldSpaceCubeEdges"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _EdgeWidth ("Edge Width (World Units)", Range(0.001, 0.5)) = 0.05
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
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
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 localPos : TEXCOORD1;
            };
            
            float4 _EdgeColor;
            float _EdgeWidth;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz;
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                // Berechne die Weltgröße des Objekts in jeder Achse
                float3 worldSize = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                
                // Konvertiere die lokale Position (-0.5 bis 0.5) in Weltkoordinaten relativ zur Objektgröße
                float3 normalizedPos = (i.localPos + 0.5); // 0 bis 1
                
                // Berechne den Abstand zu den Kanten in Welteinheiten
                float3 worldEdgeDistance = float3(
                    min(normalizedPos.x * worldSize.x, (1.0 - normalizedPos.x) * worldSize.x),
                    min(normalizedPos.y * worldSize.y, (1.0 - normalizedPos.y) * worldSize.y),
                    min(normalizedPos.z * worldSize.z, (1.0 - normalizedPos.z) * worldSize.z)
                );
                
                float smoothness = _EdgeWidth * 0.5;
                
                // X-Kanten (parallel zur X-Achse) - konstant in Y und Z
                float xEdge = (worldEdgeDistance.y < _EdgeWidth && worldEdgeDistance.z < _EdgeWidth) ? 
                    smoothstep(_EdgeWidth + smoothness, _EdgeWidth - smoothness, max(worldEdgeDistance.y, worldEdgeDistance.z)) : 0.0;
                
                // Y-Kanten (parallel zur Y-Achse) - konstant in X und Z
                float yEdge = (worldEdgeDistance.x < _EdgeWidth && worldEdgeDistance.z < _EdgeWidth) ? 
                    smoothstep(_EdgeWidth + smoothness, _EdgeWidth - smoothness, max(worldEdgeDistance.x, worldEdgeDistance.z)) : 0.0;
                
                // Z-Kanten (parallel zur Z-Achse) - konstant in X und Y
                float zEdge = (worldEdgeDistance.x < _EdgeWidth && worldEdgeDistance.y < _EdgeWidth) ? 
                    smoothstep(_EdgeWidth + smoothness, _EdgeWidth - smoothness, max(worldEdgeDistance.x, worldEdgeDistance.y)) : 0.0;
                
                float finalEdge = saturate(xEdge + yEdge + zEdge);
                
                return float4(_EdgeColor.rgb, finalEdge * _EdgeColor.a);
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/Diffuse"
}