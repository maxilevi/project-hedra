#version 330 compatibility

layout(location = 0)out vec4 OutColor;

in vec2 UVs;

in float YCoord;

uniform vec3 Tint;
uniform sampler2D Sampler;
uniform sampler2D Mask;
uniform sampler2D Reflect;
uniform bvec2 Bools;
uniform float Cooldown;
uniform float Time;
uniform float ReflectionPower;

void main(){
	bool UseGrayScale = Bools.x;
	bool UseMask = Bools.y;

	vec4 RGBA = texture(Sampler, UVs) * vec4(Tint, 1.0);
	
	float GrayScale = (RGBA.r + RGBA.b + RGBA.g) / 3.0;
	vec4 Gray = vec4(vec3(GrayScale, GrayScale, GrayScale), RGBA.a);
	if(Tint != vec3(0.0, 0.0, 0.0))
		RGBA = (RGBA + Gray) * 0.5 * vec4(Tint, 1.0);
	
	if(UseGrayScale){
		OutColor = Gray * vec4(0.6, 0.6, 0.6, 1.0);
		if(UseMask)
			OutColor.a -= texture(Mask, UVs).a;
	}else{
		OutColor = vec4(mix(RGBA, Gray * vec4(.445, .48, .8, 1.0), clamp(YCoord - 1.0 * (1.0-Cooldown), 0.0, 1.0) ));
		if(UseMask)
			OutColor.a -= texture(Mask, UVs).a;
	}

}
