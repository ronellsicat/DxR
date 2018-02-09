// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Star Nest algorithm by Pablo Román Andrioli
//Unity 5.x shader by Jonathan Cohen
//This content is under the MIT License.
//
//Original Shader:
//https://www.shadertoy.com/view/XlfGRj
//
//This shader uses the same algorithm in 3d space to render a skybox.

Shader "Skybox/StarNest" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		
		[Toggle(CLAMPOUT)] _CLAMPOUT("Clamp Output with Main Color", Float) = 0
		
		//Scrolls in this direction over time. Set 'w' to zero to stop scrolling.
		_Scroll ("Scrolling direction (x,y,z) * w * time", Vector) = (1.3, 1, .6, .01)
		
		//Center position in space and time.
		_Center ("Center Position (x, y, z, time)", Vector) = (1, .3, .5, 0)
		
		//How much does camera position cause the effect to scroll?
		_CamScroll ("Camera Scroll", Float) = 0
		
		//Does rotation apply?
		_Rotation ("Rotation axis (x,y,z) * w * time", Vector) = (0, 0, 0, .01)
		
		//Iterations of inner loop. 
		//The higher this is, the more distant objects get rendered.
		_Iterations ("Iterations", Range(1, 30)) = 15
		
		//Volumetric rendering steps. Each 'step' renders more objects at all distances.
		//This has a higher performance hit than iterations.
		_Volsteps ("Volumetric Steps", Range(1,20)) = 8
		
		//Magic number. Best values are around 400-600.
		_Formuparam ("Formuparam", Float) = 420
		
		//How much farther each volumestep goes
		_StepSize ("Step Size", Float) = 355
		
		//Fractal repeating rate
		//Low numbers are busy and give lots of repititio
		//High numbers are very sparce
		_Tile ("Tile", Float) = 700
		
		//Brightness scale.
		_Brightness ("Brightness", Float) = .5
		//Abundance of Dark matter (in the distance). 
		//Visible with Volsteps >= 8 (at 7 its really, really hard to see)
		_Darkmatter ("Dark Matter", Float) = 555
		//Brightness of distant objects (or dim) are distant objects
		//Ironically, Also affets brightness of 'darkmatter'
		_Distfading ("Distance Fading", Float) = 55
		//How much color is present?
		_Saturation ("Saturation", Float) = 77
		
	}

	SubShader {
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ CLAMPOUT
			#include "UnityCG.cginc"
			
			static const float kInnerRadius = 1.0;
			static const float kCameraHeight = 0.0001;

			fixed4 _Color;
			
			int _Volsteps;
			int _Iterations;
			
			float4 _Scroll;
			float4 _Center;
			float _CamScroll;
			float4 _Rotation;
			
			float _Formuparam;
			float _StepSize;

			float _Tile;

			float _Brightness;
			float _Darkmatter;
			float _Distfading;
			float _Saturation;
			
			struct appdata_t {
				float4 vertex : POSITION;
			};
			struct v2f {
				float4 pos : SV_POSITION;
				half3 rayDir : TEXCOORD0;	// Vector for incoming ray, normalized ( == -eyeRay )
			}; 
			
			v2f vert(appdata_t v) {
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				float3 cameraPos = float3(0,kInnerRadius + kCameraHeight,0); 	// The camera's current position
			
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

				OUT.rayDir = half3(eyeRay);
				
				
				return OUT;
			}
			
			half4 frag (v2f IN) : SV_Target {
				half3 col = half3(0, 0, 0);
				float4 pos = IN.pos;
				half3 dir = IN.rayDir;
				
				float time = _Center.w + _Time.x;
				
				//Un-scale parameters (source parameters for these are mostly in 0...1 range)
				//Scaling them up makes it much easier to fine-tune shader in the inspector.
				float brightness = _Brightness / 1000;
				float stepSize = _StepSize / 1000;
				float3 tile = abs(float3(_Tile, _Tile, _Tile)) / 1000;
				float formparam = _Formuparam / 1000;
				
				float darkmatter = _Darkmatter / 100;
				float distFade = _Distfading / 100;
				
				float3 from = _Center.xyz;
				
				//scroll over time
				from += _Scroll.xyz * _Scroll.w * time;
				//scroll from camera position
				from += _WorldSpaceCameraPos * _CamScroll * .00001;
				
				
				//Apply rotation if enabled
				float3 rot = _Rotation.xyz * _Rotation.w * time * .1;
				if (length(rot) > 0) {
					float2x2 rx = float2x2(cos(rot.x), sin(rot.x), -sin(rot.x), cos(rot.x));
					float2x2 ry = float2x2(cos(rot.y), sin(rot.y), -sin(rot.y), cos(rot.y));
					float2x2 rz = float2x2(cos(rot.z), sin(rot.z), -sin(rot.z), cos(rot.z));

					dir.xy = mul(rz, dir.xy);
					dir.xz = mul(ry, dir.xz);
					dir.yz = mul(rx, dir.yz);
					from.xy = mul(rz, from.xy);
					from.xz = mul(ry, from.xz);
					from.yz = mul(rx, from.yz);
				}
				
				
				//volumetric rendering
				float s = 0.1, fade = 1.0;
				float3 v = float3(0, 0, 0);
				for (int r = 0; r < _Volsteps; r++) {
					float3 p = abs(from + s * dir * .5);
					
					p = abs(float3(tile - fmod(p, tile*2)));
					float pa,a = pa = 0.;
					for (int i = 0; i < _Iterations; i++) {
						p = abs(p) / dot(p, p) - formparam;
						a += abs(length(p) - pa);
						pa = length(p);
					}
					//Dark matter
					float dm = max(0., darkmatter - a * a * .001);
					if (r > 6) { fade *= 1. - dm; } // Render distant darkmatter
					a *= a * a; //add contrast
					
					v += fade;
					
					// coloring based on distance
					v += float3(s, s*s, s*s*s*s) * a * brightness * fade;
					
					// distance fading
					fade *= distFade;
					s += stepSize;
				}
				
				float len = length(v);
				//Quick saturate
				v = lerp(float3(len, len, len), v, _Saturation / 100);
				v.xyz *= _Color.xyz * .01;
				
				#ifdef CLAMPOUT
					v = clamp(v, float3(0,0,0), _Color.xyz);
				#endif
				return float4(v, 1.0);
			}
			
			
			
			
			ENDCG
		}
		
		
	}

	Fallback Off
}