Shader "Custom/ScreenSpaceCubeEdges"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _EdgeWidth ("Edge Width (Pixels)", Range(20, 100)) = 20
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
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 objectPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };
            
            float4 _EdgeColor;
            float _EdgeWidth;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.objectPos = v.vertex.xyz;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                // Normalisiere die Objektposition auf 0-1 Bereich
                float3 pos = (i.objectPos + 0.5);
                
                // Berechne die Weltgröße eines Pixels an dieser Position
                float4 worldPos = mul(unity_ObjectToWorld, float4(i.objectPos, 1.0));
                float distanceToCamera = length(_WorldSpaceCameraPos - worldPos.xyz);
                
                // Berechne wie groß ein Pixel in Weltkoordinaten ist
                float pixelSize = (distanceToCamera * 2.0 * tan(radians(unity_CameraProjection._m11) * 0.5)) / _ScreenParams.y;
                
                // Berechne die Objektskalierung
                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                
                // Konvertiere die gewünschte Pixel-Breite in Objekt-Koordinaten
                float edgeWidthInObject = (_EdgeWidth * pixelSize) / min(min(scale.x, scale.y), scale.z);
                
                // Berechne Abstand zu den Kanten für jede Achse
                float edgeX = min(pos.x, 1.0 - pos.x);
                float edgeY = min(pos.y, 1.0 - pos.y);
                float edgeZ = min(pos.z, 1.0 - pos.z);
                
                float smoothness = edgeWidthInObject * 0.5;
                
                // X-Kanten (parallel zur X-Achse)
                float xEdge = step(edgeY, edgeWidthInObject) * step(edgeZ, edgeWidthInObject);
                xEdge *= smoothstep(edgeWidthInObject + smoothness, edgeWidthInObject, max(edgeY, edgeZ));
                
                // Y-Kanten (parallel zur Y-Achse)  
                float yEdge = step(edgeX, edgeWidthInObject) * step(edgeZ, edgeWidthInObject);
                yEdge *= smoothstep(edgeWidthInObject + smoothness, edgeWidthInObject, max(edgeX, edgeZ));
                
                // Z-Kanten (parallel zur Z-Achse)
                float zEdge = step(edgeX, edgeWidthInObject) * step(edgeY, edgeWidthInObject);
                zEdge *= smoothstep(edgeWidthInObject + smoothness, edgeWidthInObject, max(edgeX, edgeY));
                
                float finalEdge = saturate(xEdge + yEdge + zEdge);
                
                return float4(_EdgeColor.rgb, finalEdge * _EdgeColor.a);
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/Diffuse"
}