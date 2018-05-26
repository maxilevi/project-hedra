#version 330 compatibility

!include<"Includes/GammaCorrection.shader">
!include<"Includes/Conditionals.shader">
!include<"Includes/Lighting.shader">

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;
layout(location = 2)in vec3 InNormal;

out vec4 InPos;
smooth out vec4 InNorm;
out vec4 Color;
out float Visibility;
out float Height;
out vec4 BotColor;
out vec4 TopColor;
out vec4 Coords;
out vec3 LightDir;
out float CastShadows;
out float Config;
out float DitherVisibility;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

uniform vec3 PlayerPosition;
uniform float Time;
uniform float Fancy = 1.0;
uniform mat4 ShadowMVP;
uniform float ShadowDistance;
uniform float UseShadows = 3.0;
uniform vec3 Scale;
uniform vec3 Offset;
uniform vec3 BakedOffset;
uniform mat4 TransformationMatrix;
uniform float DitherRadius;
const float ShadowTransition = 10.0;

vec2 Unpack(float inp, int prec)
{
    vec2 outp = vec2(0.0, 0.0);

    outp.y = mod(inp, prec);
    outp.x = floor(inp / prec);

    return outp / (prec - 1.0);
}

void main(){
	Config = InColor.a;
	CastShadows = InColor.a;
	Color = vec4(srgb_to_linear(InColor.xyz), InColor.a);
	vec3 unitToLight = normalize(LightPosition);
	vec4 Vertex = vec4((InVertex + BakedOffset) * Scale + Offset, 1.0);
	
	float config_set = when_ge(InColor.a, 0.0) * Fancy; //If configuration is set
	vec2 Unpacked = Unpack(InColor.a, int(2048.0));
	float Addon = config_set * ( cos(Time + Unpacked.y * 8.0) +0.8) * .85 * 0.7 * Unpacked.x * 1.2;

	float invert_uk = when_lt(Unpacked.y, 0.5);
	Vertex.x += invert_uk * Addon * Scale.x;
	Vertex.z -= invert_uk * Addon * Scale.z;
	Vertex.x -= not(invert_uk) * Addon * Scale.x;
	Vertex.z += not(invert_uk) * Addon * Scale.z;

	Height = U_Height;
	BotColor = U_BotColor;
	TopColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	DitherVisibility = clamp( (DitherRadius - DistanceToCamera) / DitherRadius, 0.0, 1.0);

	Vertex = TransformationMatrix * Vertex;
	gl_Position = gl_ModelViewProjectionMatrix * Vertex;

	float use_shadows = when_neq(UseShadows, 0.0) * when_neq(InColor.a, -1.0);

	float ShadowDist = DistanceToCamera - (ShadowDistance - ShadowTransition);
	ShadowDist /= ShadowTransition;
	Coords = use_shadows * ShadowMVP * vec4(InVertex,1.0);
	Coords.w = use_shadows * clamp(1.0 - ShadowDist, 0.0, 1.0);
	LightDir = use_shadows * unitToLight;
	
	InPos = Vertex;
	InNorm = vec4(InNormal, 1.0);
}