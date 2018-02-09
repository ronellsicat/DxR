Shader "StarTools/3DStarShader"
{
	Properties
	{
		_LocalTime("LocalTime", Float) = 0.05
		_Resolution("Resolution", Float) = 5
		_StarCenter("Star Center", Vector) = (0, 0, 0, 0)
		_StarColor("Star Color", Color) = (1, 1, 1, 1)
		_RotRate("Rotation Speed", Vector) = (0, -1, 0, 0)
		_Contrast("Contrast", Float) = 1
	}
		SubShader
	{
		Tags {  "DisableBatching" = "True"}

		/*
		"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		*/
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "noiseHelper.cginc"

			float4 _StarColor;
			float4 _StarCenter;
			float4 _RotRate;
			float _LocalTime;
			float _Resolution;
			float _Contrast;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float3 position_in_world_space : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;

				float s = sin(_RotRate.x);
				float c = cos(_RotRate.x);
				float3x3 rotationMatrix_x = float3x3(1, 0, 0, 0, c, -s, 0, s, c);
				s = sin(_RotRate.y);
				c = cos(_RotRate.y);
				float3x3 rotationMatrix_y = float3x3(c, 0, s, 0, 1, 0, -s, 0, c);
				s = sin(_RotRate.z);
				c = cos(_RotRate.z);
				float3x3 rotationMatrix_z = float3x3(c, -s, 0, s, c, 0, 0, 0, 1);

				o.vertex = mul((float4x4) unity_ObjectToWorld, v.vertex);
				o.position_in_world_space = float3(o.vertex.x, o.vertex.y, o.vertex.z) - _StarCenter.xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.position_in_world_space = mul(o.position_in_world_space, rotationMatrix_x);
				o.position_in_world_space = mul(o.position_in_world_space, rotationMatrix_y);
				o.position_in_world_space = mul(o.position_in_world_space, rotationMatrix_z);

				return o;
			}

			float star_base_noise(float3 pos, float time)
			{
				float noise1 = snoise_turbulence_additive(pos / 5, time, 5);
				float noise2 = snoise_turbulence_additive(pos / 10, time + 25, 5);
				float noise3 = snoise_turbulence_minimized(pos * min(noise1, noise2), time + 50, 2) * 2;
				float noise4 = snoise_turbulence_additive(pos, time, 3);

				float noise5 = snoise_turbulence_additive(pos / 25, time * 0.3, 5);


				float noiseFinal = (noise1 + noise2 + noise3 + noise4 - (noise5 * 1.5));
				return noiseFinal * _Contrast;
			}

			float4 frag(v2f i) : COLOR
			{
				//Need to find RGB, then a multiplier based off of apparent magnitude

				//For B-V, 0 is about pure blue and 1.5 is about pure red
				//0.5 is whiteish, 1 is yellowish.

				float3 pos_offset = i.position_in_world_space / _Resolution;

				float noise_base = (star_base_noise(pos_offset , _LocalTime, _Contrast));

				float offset = 1 * (noise_base);

				return _StarColor +float4(offset, offset, offset, 0);// -float4(thresh, thresh, thresh, 0);
			}

			ENDCG
		}
	}
}
