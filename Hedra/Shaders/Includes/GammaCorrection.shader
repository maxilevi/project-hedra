float linear_to_srbg(float c){
    float a = .055;
    if (c <= .0031308) {
        return 12.92 * c;
    } else {
        return (1.0 + a) * pow(c, 1.0/2.4) - a;
    }
}

float srgb_to_linear(float c){
	float a = .055;
	if (c <= .04045) {
		return c / 12.92;
	} else {
		return pow( (c+a) / (1+a), 2.4);
	}
}

vec3 linear_to_srbg(vec3 rgb){
	return vec3(linear_to_srbg(rgb.r), linear_to_srbg(rgb.g), linear_to_srbg(rgb.b));
}

vec3 srgb_to_linear(vec3 rgb){
	return vec3(srgb_to_linear(rgb.r), srgb_to_linear(rgb.g), srgb_to_linear(rgb.b));
}

vec4 linear_to_srbg(vec4 rgba){
	return vec4(linear_to_srbg(rgba.rgb), rgba.a);
}

vec4 srgb_to_linear(vec4 rgba){
	return vec4(srgb_to_linear(rgba.rgb), rgba.a);
}