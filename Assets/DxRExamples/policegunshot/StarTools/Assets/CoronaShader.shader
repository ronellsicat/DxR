Shader "StarTools/CoronaShader"
{
	Properties
	{
		_StarCenter("Star Center", Vector) = (0, 0, 0, 0)	//x, y, z, radius
		_StarColor("Star Color", Color) = (1, 1, 1, 1)
		//_CoronaSettings("Corona settings", Vector) = (10, 5, 0, 0)
		_LocalTime("LocalTime", Float) = 0.05
		_Resolution("Resolution", Float) = 5
		_Contrast("Contrast", Float) = 1
		_RotRate("Rotation Speed", Vector) = (0, -1, 0, 0)

	}
		SubShader
	{
		Tags{ "DisableBatching" = "True" "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"
	}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

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
	//float4 _CoronaSettings;
	float _LocalTime;
	float _Resolution;
	float4 _RotRate;
	float _Contrast;


	struct appdata
	{
		float4 vertex :	POSITION;
	};

	struct v2f
	{
		float4 vertex : POSITION;
		float3 position_in_world_space : TEXCOORD0;
		float dist : TEXCOORD1;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = mul((float4x4) unity_ObjectToWorld, v.vertex);
		o.dist = distance(o.vertex, _StarCenter.xyz);
		o.position_in_world_space = float3(o.vertex.x, o.vertex.y, o.vertex.z) - _StarCenter.xyz;
		o.vertex = UnityObjectToClipPos(v.vertex);

		float s = sin(_RotRate.x);
		float c = cos(_RotRate.x);
		float3x3 rotationMatrix_x = float3x3(1, 0, 0, 0, c, -s, 0, s, c);
		s = sin(_RotRate.y);
		c = cos(_RotRate.y);
		float3x3 rotationMatrix_y = float3x3(c, 0, s, 0, 1, 0, -s, 0, c);
		s = sin(_RotRate.z);
		c = cos(_RotRate.z);
		float3x3 rotationMatrix_z = float3x3(c, -s, 0, s, c, 0, 0, 0, 1);

		o.position_in_world_space = mul(o.position_in_world_space, rotationMatrix_x);
		o.position_in_world_space = mul(o.position_in_world_space, rotationMatrix_y);
		o.position_in_world_space = mul(o.position_in_world_space, rotationMatrix_z);

		return o;
	}

	float4 frag(v2f i) : COLOR
	{   //First get color
	float3 pos_offset = i.position_in_world_space / _Resolution;

	float noise_base = (star_base_noise(pos_offset , _LocalTime, _Contrast));
	float4 color = _StarColor + float4(noise_base, noise_base, noise_base, 0);

	//Get distance as a ratio
	float distanceFromSurface = (i.dist);
	float distanceRatio = (_StarCenter.w) / distanceFromSurface;

	noise_base = pow(distanceRatio + 0.05, 9);
	color.w = noise_base;

	return color;
	}

		ENDCG
	}
	}
}
