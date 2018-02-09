//Nvidia CG include
//Quick HSV conversions
//Jonathan Cohen
//2015
//Distributed under MIT license
//This content is under the MIT License.

#ifndef HSV_INCLUDED
#define HSV_INCLUDED
//LOL INCLUDE GUARDS

//Convert from RGB color space to HSV color space
float3 toHSV(float3 c) {
	float high = max(max(c.r, c.g), c.b);
	if (high <= .0) { return float3(0, 0, 0); }
	
	float val = high;
	float low = min(min(c.r, c.g), c.b);
	float delta = high - low;
	float sat = delta/high;
	
	float hue; //br? hue hue hue hue hue
	if (c.r == high) { 
		hue = (c.g - c.b) / delta;
	} else if (c.g == high) {
		hue = 2 + (c.b - c.r) / delta;
	} else {
		hue = 4 + (c.r - c.g) / delta;
	}
	
	hue /= 6.0;
	if (hue < 0) { hue += 1.0; }
	else if (hue > 1) { hue -= 1.0; }
	
	return float3(hue, sat, val);
}

//Convert from HSV color space to RGB color space
float3 toRGB(float3 hsv) {
	int i;
	float f, p, q, t;
	float h = fmod(hsv.r, 1.0);
	float s = hsv.g;
	float v = hsv.b;
	
	if (s == 0) { return float3(v, v, v); }
	
	h *= 6.0;
	i = int(floor(h));
	f = h - i;
	p = v * (1.0-s);
	q = v * (1.0-s * f);
	t = v * (1.0 - s * (1.0 - f) );
	
	if (i == 0) { return float3(v, t, p); }
	else if (i == 1) { return float3(q, v, p); }
	else if (i == 2) { return float3(p, v, t); }
	else if (i == 3) { return float3(p, q, v); }
	else if (i == 4) { return float3(t, p, v); }
	
	return float3(v, p, q);
	
}

#endif