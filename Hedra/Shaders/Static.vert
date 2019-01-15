#version 330 core

!include<"Includes/GammaCorrection.shader">
!include<"Includes/Lighting.shader">

precision lowp float;

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;
layout(location = 2)in vec3 InNormal;

out vec4 raw_color;
out vec4 Color;
out vec4 InPos;
out vec4 InNorm;
out vec3 pointlight_color;
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;
out vec4 Coords;
out vec3 LightDir;
out float DitherVisibility;
out vec3 base_vertex_position;
out float use_shadows;
out float shadow_quality;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

uniform vec3 PlayerPosition;
uniform vec4 TimeFancyShadowDistanceUseShadows;
uniform mat4 ShadowMVP;
uniform vec3 Scale;
uniform vec3 Offset;
uniform vec3 BakedOffset;
uniform mat4 TransformationMatrix;
uniform float MinDitherDistance;
uniform float MaxDitherDistance;
const float ShadowTransition = 10.0;
const float NoShadowsFlag = -1.0;
const float NoHighlightFlag = -2.0;
const float FlagEpsilon = 0.1;

float when_neq(float x, float y)
{
	return abs(sign(x - y));
}

float when_lt(float x, float y)
{
	return max(sign(y - x), 0.0);
}

float when_ge(float x, float y)
{
	return 1.0 - when_lt(x, y);
}

float not(float x){
	return 1.0 - x;
}

vec2 Unpack(float inp, int prec)
{
    vec2 outp = vec2(0.0, 0.0);

    outp.y = mod(inp, prec);
    outp.x = floor(inp / prec);

    return outp / (prec - 1.0);
}

void main()
{
    vec4 linear_color = srgb_to_linear(InColor); 
	float Config = InColor.a;
	vec3 unitToLight = normalize(LightPosition);
	vec4 Vertex = vec4((InVertex + BakedOffset) * Scale + Offset, 1.0);
	base_vertex_position = Vertex.xyz;
	
	float config_set = when_ge(InColor.a, 0.0) * TimeFancyShadowDistanceUseShadows.y;
	vec2 Unpacked = Unpack(InColor.a, int(2048.0));
	float Addon = config_set * ( cos(TimeFancyShadowDistanceUseShadows.x + Unpacked.y * 8.0) +0.8) * .85 * 0.7 * Unpacked.x * 1.2;

	float invert_uk = when_lt(Unpacked.y, 0.5);
	Vertex.x += invert_uk * Addon * Scale.x;
	Vertex.z -= invert_uk * Addon * Scale.z;
	Vertex.x -= not(invert_uk) * Addon * Scale.x;
	Vertex.z += not(invert_uk) * Addon * Scale.z;

	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	DitherVisibility = clamp( (MaxDitherDistance - DistanceToCamera) / (MaxDitherDistance - MinDitherDistance), 0.0, 1.0);

	Vertex = TransformationMatrix * Vertex;
	gl_Position = _modelViewProjectionMatrix * Vertex;

	use_shadows = when_neq(TimeFancyShadowDistanceUseShadows.w, 0.0) * when_neq(InColor.a, NoShadowsFlag);
    shadow_quality = TimeFancyShadowDistanceUseShadows.w;
	float ShadowDist = DistanceToCamera - (TimeFancyShadowDistanceUseShadows.z - ShadowTransition);
	ShadowDist /= ShadowTransition;
	Coords = use_shadows * ShadowMVP * vec4(InVertex,1.0);
	Coords.w = use_shadows * clamp(1.0 - ShadowDist, 0.0, 1.0);
	LightDir = use_shadows * unitToLight;
	
	InPos = Vertex;
	InNorm = vec4(InNormal, 1.0);
    if(Config - NoHighlightFlag > FlagEpsilon)
    {
        linear_color = apply_highlights(linear_color, Vertex.xyz);
    }
		
	//Lighting
	vec3 unitNormal = normalize(InNorm.xyz);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - Vertex.xyz);

	vec3 FLightColor = calculate_lights(LightColor, Vertex.xyz);
	vec4 InputColor = vec4(linear_color.xyz, 1.0);

	Ambient += Config - NoShadowsFlag < FlagEpsilon ? 0.25 : 0.0;
	vec4 Rim = rim(InputColor.rgb, LightColor, unitToCamera, unitNormal);
	vec4 Diffuse = diffuse(unitToLight, unitNormal, LightColor);
	vec4 realColor = Rim + Diffuse * InputColor;
    Color = vec4(realColor.xyz, realColor.a);	

	pointlight_color = diffuse(unitToLight, unitNormal, FLightColor).rgb;		
	raw_color = linear_color;
	
}