#ifndef NOISE_HELPER
#define NOISE_HELPER

#include "WebGLNoisePort/noiseSimplex.cginc"

#define PI 3.141592653589793

float snoise_n1_to_1(float4 pos)
{
	return 2 * (snoise(pos) - 0.5);
}

float snoise_n1_to_1(float3 pos, float time)
{
	return snoise_n1_to_1(float4(pos.xyz, time));
}

float snoise_turbulence_multiplicative(float3 pos, float time, int raised)
{
	float val = 0;
	for (int i = 1; i <= raised; i++)
	{
		float power = pow(2.0, float(i + 1));
		val *= (0.5 / power) * snoise(float4(pos.xyz * power, time));
	}
	return val;
}

float snoise_turbulence_multiplicative(float2 pos, float time, int raised)
{
	float val = 0;
	for (int i = 1; i <= raised; i++)
	{
		float power = pow(2.0, float(i + 1));
		val *= (0.5 / power) * snoise(float3(pos.xy * power, time));
	}
	return val;
}

float snoise_turbulence_additive(float3 pos, float time, int raised)
{
	float val = 0;
	for (int i = 1; i <= raised; i++)
	{
		float power = pow(2.0, float(i + 1));
		val += (0.5 / power) * snoise(float4(pos.xyz * power, time));
	}
	return val;
}

float snoise_turbulence_additive(float2 pos, float time, int raised)
{
	float val = 0;
	for (int i = 1; i <= raised; i++)
	{
		float power = pow(2.0, float(i + 1));
		val += (0.5 / power) * snoise(float3(pos.xy * power, time));
	}
	return val;
}

float snoise_turbulence_minimized(float3 pos, float time, int raised)
{
	float val = 0;
	for (int i = 1; i <= raised; i++)
	{
		float power = pow(2.0, float(i + 1));
		val = min(val, (0.5 / power) * snoise(float4(pos.xyz * power, time)));
	}
	return val;
}

float snoise_turbulence_minimized(float2 pos, float time, int raised)
{
	float val = 0;
	for (int i = 1; i <= raised; i++)
	{
		float power = pow(2.0, float(i + 1));
		val = min(val, (0.5 / power) * snoise(float3(pos.xy * power, time)));
	}
	return val;
}

float star_base_noise(float3 pos, float time, float contrast)
{
	float noise1 = snoise_turbulence_additive(pos / 5, time, 5);
	float noise2 = snoise_turbulence_additive(pos / 10, time + 25, 5);
	float noise3 = snoise_turbulence_minimized(pos * min(noise1, noise2), time + 50, 2) * 2;
	float noise4 = snoise_turbulence_additive(pos, time, 3);

	float noise5 = snoise_turbulence_additive(pos / 25, time * 0.3, 5);


	float noiseFinal = (noise1 + noise2 + noise3 + noise4 - (noise5 * 1.5));
	return noiseFinal * contrast;
}

#endif