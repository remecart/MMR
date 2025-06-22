// Optimized ObstacleGrabpass Shader
// Performance improvements: removed triplanar mapping, simplified distortion, reduced texture samples
Shader "ObstacleGrabpass_Optimized"
{
	Properties
	{
		_DistortionMap( "Distortion Map", 2D ) = "bump" {}
		_DistortionIntensity( "DistortionIntensity", Range(0, 0.1) ) = 0.025
		_MainColor( "Main Color", Color ) = ( 1, 0, 0, 0.2078431 )
		_ADDColor( "ADD Color", Color ) = ( 0.1320754, 0, 0, 0.4392157 )
		_UVScale( "UV Scale", Float ) = 0.1
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 0

		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual

		GrabPass{ "_BackgroundTexture" }

		Pass
		{
			Name "Unlit"

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal : NORMAL;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 grabPos : TEXCOORD0;
					float2 uv : TEXCOORD1;
					float3 viewDir : TEXCOORD2;
					float3 normal : TEXCOORD3;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				sampler2D _BackgroundTexture;
				sampler2D _DistortionMap;
				float4 _DistortionMap_ST;
				uniform float _UVScale;
				uniform float _DistortionIntensity;
				uniform float4 _ADDColor;
				uniform float4 _MainColor;

				v2f vert ( appdata v )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID( v );
					UNITY_TRANSFER_INSTANCE_ID( v, o );

					o.pos = UnityObjectToClipPos( v.vertex );
					o.grabPos = ComputeGrabScreenPos( o.pos );
					
					// Simple UV mapping instead of triplanar
					o.uv = TRANSFORM_TEX(v.uv, _DistortionMap) * _UVScale;
					
					// Simplified view direction calculation
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
					o.normal = UnityObjectToWorldNormal(v.normal);

					return o;
				}

				half4 frag( v2f i ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );

					// Sample normal map only once
					float3 normalMap = UnpackNormal(tex2D(_DistortionMap, i.uv));
					
					// Simplified distortion calculation
					float2 distortion = normalMap.xy * _DistortionIntensity;
					
					// Apply distortion to grab coordinates
					float2 grabUV = (i.grabPos.xy / i.grabPos.w) + distortion;
					
					// Sample background texture
					half4 screenColor = tex2D(_BackgroundTexture, grabUV);
					
					// Simplified color blending
					half4 additive = _ADDColor * _ADDColor.a;
					half4 result = lerp(screenColor + additive, _MainColor, _MainColor.a);
					
					// Use original alpha
					result.a = screenColor.a;

					return result;
				}
			ENDCG
		}
	}
	
	Fallback "Sprites/Default"
}