Shader "Note"
{
    Properties
    {
        _Color("Main Color", Color) = (0.25, 0.25, 0.25, 1)
        _CubeMap("CubeMap", CUBE) = "white" {}
        _Transparent("Transparent", Float) = 0
        _ScreenspaceTexture("Screenspace Texture", 2D) = "white" {}
        _ScreenspaceIntensity("Screenspace Intensity", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IsEmissive" = "true" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off

        Pass
        {
            Name "Unlit"
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _CubeMap;
            sampler2D _ScreenspaceTexture;
            float4 _Color;
            float _Transparent;
            float _ScreenspaceIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 reflection = reflect(-viewDir, normalize(i.worldNormal));
                float4 screenColor = tex2D(_ScreenspaceTexture, i.screenPos.xy / i.screenPos.w);
                float4 baseColor = _Color;

                float3 emissiveColor;
                float alphaVal;

                if (_Transparent == 1.0)
                {
                    baseColor *= 0.1;
                    emissiveColor = screenColor.rgb * baseColor.rgb * _ScreenspaceIntensity;

                    // Make black pixels transparent
                    float luminance = dot(emissiveColor, float3(0.299, 0.587, 0.114));
                    alphaVal = (luminance <= 0.001) ? 0.0 : 1.0;
                }
                else
                {
                    emissiveColor = texCUBE(_CubeMap, reflection).rgb * baseColor.rgb * 0.492695;
                    alphaVal = 1.0;
                }

                return float4(emissiveColor, alphaVal);
            }
            ENDCG
        }
    }
    Fallback Off
}